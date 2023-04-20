using AutoMapper;
using CT554_API.Models;
using CT554_Entity.Data;
using CT554_Entity.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CT554_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(policy: "Admin")]
    public class invoicesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly CT554DbContext _context;
        private readonly ILogger<invoicesController> _logger;
        public invoicesController(CT554DbContext context, IMapper mapper, ILogger<invoicesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice([FromRoute] Guid id)
        {
            var invoice = await _context.Invoices.Include(invoice => invoice.Vender)!
                .Include(invoice => invoice.Details)!.ThenInclude(detail => detail.ProductDetail)
                .ThenInclude(productDetail => productDetail!.Product)
                .AsSplitQuery()
                .Include(invoice => invoice.Details)!.ThenInclude(detail => detail.ProductDetail)
                .ThenInclude(productDetail => productDetail!.Prices)
                .FirstOrDefaultAsync(Invoice => Invoice.Id == id)
                ?? throw new Exception();

            foreach (var detail in invoice.Details!)
            {
                detail.ProductDetail!.TargetDate = invoice.DateCreate;
            }

            return Ok(_mapper.Map<InvoiceInfo>(invoice));
        }

        [HttpGet]
        public async Task<IActionResult> Getinvoices([FromQuery] string venderName = "", [FromQuery] string venderId = "0" , [FromQuery] int productId = 0, [FromQuery] string fromDate = "",
            [FromQuery] string toDate = "", [FromQuery] int page = 1,
            [FromQuery] float fromPrice = -1, [FromQuery] float toPrice = -1,
            [FromQuery] int size = 5, [FromQuery] string sort = "", [FromQuery] string order = "")
        {
            var query = _context.Invoices.AsQueryable();

            if(venderName != "") {
                query = query.Where(invoice => invoice.Vender.Name.ToLower().Contains(venderName));
            }

            if (venderId != "0")
            {
                query = query.Where(c => c.VenderId == Guid.Parse(venderId));
            }
            if (productId != 0)
                {
                    query = query.Where(c => c.Details!.Any(detail => detail.ProductDetail!.ProductId == productId));
                }

            if (fromPrice != -1)
            {
                query = query.Where(c => c.RealTotal >= fromPrice);
            }
            if (toPrice != -1)
            {
                query = query.Where(c => c.RealTotal <= toPrice);
            }

            if (fromDate != "")
            {
                var fromDateParsed = DateTime.Parse(fromDate);
                query = query.Where(invoice => invoice.DateCreate.AddHours(7).Date.CompareTo(fromDateParsed.Date) >= 0);
            }

            if(toDate != "")
            {
                var toDateParsed = DateTime.Parse(toDate);
                query = query.Where(invoice => invoice.DateCreate.AddHours(7).Date.CompareTo(toDateParsed.Date) <= 0);
            }

            var totalRows = query.Count();

            var invoices = await query.Include(invoice => invoice.Vender)
                .Include(invoice => invoice.Details)!.ThenInclude(detail => detail.ProductDetail)
                .ThenInclude(productDetail => productDetail!.Prices)
                .ToListAsync();


            foreach (var invoice in invoices)
            {
                foreach (var detail in invoice.Details!)
                {
                    detail.ProductDetail!.TargetDate = invoice.DateCreate;
                }
            }

            invoices = order.ToLower() switch
            {
                "desc" => sort.ToLower() switch
                {
                    "total" => invoices.OrderByDescending(c => c.RealTotal).ToList(),
                    "datecreate" => invoices.OrderByDescending(c => c.DateCreate).ToList(),
                    _ => invoices.OrderByDescending(c => c.Id).ToList()
                },
                _ => sort.ToLower() switch
                {
                    "total" => invoices.OrderBy(c => c.RealTotal).ToList(),
                    "datecreate" => invoices.OrderBy(c => c.DateCreate).ToList(),
                    _ => invoices.OrderBy(c => c.Id).ToList()
                }
            };

            return Ok(
                new
                {
                    invoices = _mapper.Map<IEnumerable<InvoiceInfo>>(invoices.Skip((page - 1) * size).Take(size)),
                    totalRows,
                    totalPages = (totalRows - 1) / size + 1,
                });
        }

        [HttpPost]
        public async Task<IActionResult> PostInvoice([FromBody] InvoiceDTO model)
        {
            var invoiceToAdd = new Invoice
            {
                VenderId = model.VenderId,
                RealTotal = model.RealTotal
            };
            _context.Invoices.Add(invoiceToAdd);
            await _context.SaveChangesAsync();

            var invoiceDetails = model.InvoiceDetails.Select(detail => new InvoiceDetail()
            {
                InvoiceId = invoiceToAdd.Id,
                ProductDetailId = detail.ProductDetailId,
                Quantity = detail.Quantity
            });
            _context.InvoiceDetails.AddRange(invoiceDetails);
            await _context.SaveChangesAsync();

            foreach (var detail in model.InvoiceDetails)
            {
                var lastestUpdate = await _context.Stocks.OrderByDescending(stock => stock.DateUpdate).FirstOrDefaultAsync(stock => stock.ProductDetailId == detail.ProductDetailId);
                if (lastestUpdate != null && lastestUpdate.DateUpdate.Date.CompareTo(DateTime.UtcNow.Date) == 0)
                {
                    _logger.LogError($"{detail.ProductDetailId} is has stock {lastestUpdate.Value}, will update");
                    if (lastestUpdate.IsManualUpdate)
                    {
                        _context.Stocks.Add(new Stock
                        {
                            ProductDetailId = lastestUpdate.ProductDetailId,
                            Value = lastestUpdate!.Value + detail.Quantity
                        });
                    }
                    else
                    {
                        _context.Update(lastestUpdate);
                        lastestUpdate.Value += detail.Quantity;
                    }
                }
                else
                {
                    _context.Stocks.Add(new Stock
                    {
                        ProductDetailId = detail.ProductDetailId,
                        Value = lastestUpdate?.Value ?? 0 + detail.Quantity
                    });
                }

            }

            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, _mapper.Map<InvoiceInfo>(invoiceToAdd));
        }

        [HttpGet("statistic/cost")]
        public async Task<IActionResult> GetOverallCost()
        {
            var invoicesInThisMonth = await _context.Invoices.Where(invoice => invoice.DateCreate >= DateTime.UtcNow.AddDays(-30)).ToListAsync();
            var invoicesInThisWeek = invoicesInThisMonth.Where(invoice => invoice.DateCreate >= DateTime.UtcNow.AddDays(-7)).ToList();
            var incoicesInToday = invoicesInThisWeek.Where(invoice => invoice.DateCreate.ToLocalTime().Date == DateTime.Now.Date);
            return Ok(new
            {
                thisMonth = invoicesInThisMonth.Sum(order => order.RealTotal),
                thisWeek = invoicesInThisWeek.Sum(order => order.RealTotal),
                today = incoicesInToday.Sum(order => order.RealTotal)
            });
        }
    }
}
