using AutoMapper;
using CT554_API.Models;
using CT554_Entity.Data;
using CT554_Entity.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CT554_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(policy: "Admin")]
    public class StocksController : ControllerBase
    {
        private readonly CT554DbContext _context;
        private readonly IMapper _mapper;

        public StocksController(CT554DbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> PostManualStock([FromBody] StockDTO model)
        {
            var stockToAdd = _mapper.Map<Stock>(model);
            var stockLasted = await _context.Stocks.Where(stock => stock.ProductDetailId == model.ProductDetailId)
                .OrderByDescending(stock => stock.DateUpdate).FirstOrDefaultAsync();

            stockToAdd.ManualValue = (stockLasted?.Value ?? 0) - stockToAdd.Value;
            _context.Stocks.Add(stockToAdd);
            await _context.SaveChangesAsync();
            return Ok(stockToAdd);
        }

        [HttpGet("{id}/changes")]
        public async Task<IActionResult> GetStockChangeOfProduct([FromRoute] int id, [FromQuery] string fromDate = "",
            [FromQuery] string toDate = "", [FromQuery] int page = 1, [FromQuery] int size = 5)
        {
            List<StockCombineModel> changes = new();
            var product = await _context.ProductDetails
                .Include(product => product.Carts)!.ThenInclude(cart => cart.Order)
                .AsSplitQuery()
                .Include(product => product.InvoiceDetails)!.ThenInclude(invoice => invoice.Invoice)
                .AsSplitQuery()
                .Include(product => product.Stocks)
                .FirstOrDefaultAsync(product => product.Id == id) ?? throw new Exception("Not found");
            var manualStocks = product.Stocks?.Where(stock => stock.IsManualUpdate);
            var carts = product.Carts?.Where(cart => cart.OrderId != null && cart.Order?.DateProccesed != null);
            var invoices = product.InvoiceDetails;
            changes.AddRange(_mapper.Map<IEnumerable<StockCombineModel>>(manualStocks));
            changes.AddRange(_mapper.Map<IEnumerable<StockCombineModel>>(carts));
            changes.AddRange(_mapper.Map<IEnumerable<StockCombineModel>>(invoices));

            if (fromDate != "")
            {
                var toDateParsed = DateTime.Parse(toDate);
                var fromDateParsed = toDate == "" ? DateTime.UtcNow : DateTime.Parse(fromDate);
                changes = changes.Where(change =>
                    change.DateUpdate.Date.CompareTo(DateTime.Parse(fromDate).Date) >= 0 &&
                    change.DateUpdate.Date.CompareTo(DateTime.Parse(toDate).Date) <= 0
                ).ToList();
            }

            changes = changes.OrderByDescending(change => change.DateUpdate).ToList();
            var totalRows = changes.Count;

            return Ok(
                new
                {
                    changes = changes.Skip((page - 1) * size).Take(size),
                    totalRows,
                    totalPages = (totalRows - 1) / size + 1,
                });
        }
    }
}
