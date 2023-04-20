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
    [Authorize(policy: "Admin")]
    [ApiController]
    public class ProductDetailsController : ControllerBase
    {
        private readonly ILogger<ProductDetailsController> _logger;
        private readonly IMapper _mapper;
        private readonly CT554DbContext _context;

        public ProductDetailsController(ILogger<ProductDetailsController> logger, CT554DbContext context, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
        }


        [HttpGet()]
        public async Task<IActionResult> GetProductDetails([FromQuery] int productId, [FromQuery] bool isActive = true, [FromQuery] bool isInStock = false)
        {
            var details = _context.ProductDetails.Where(d => d.ProductId == productId);
            if (isInStock)
            {
                details = details.Where(d => d.Stocks != null && d.Stocks.Any() && d.Stocks.OrderByDescending(s => s.DateUpdate).FirstOrDefault()!.Value > 0);
            }

            if (isActive)
            {
                details = details.Where(d => d.IsAvailable == isActive);
            };
            return Ok(_mapper.Map<IEnumerable<ProductDetailInfo>>(
                await details.Include(detail => detail.Prices).Include(detail => detail.Stocks).ToListAsync())
            );
        }


        [HttpPost]
        public async Task<IActionResult> PostProductDetail([FromBody] ProductDetailModel detail)
        {
            var newDetail = new ProductDetail()
            {
                Unit = detail.Unit,
                Description = detail.Description,
                IsAvailable = detail.IsAvailable,
                ToWholesale = detail.ToWholesale,
                ProductId = detail.ProductId,
            };

            await _context.ProductDetails.AddAsync(newDetail);
            await _context.SaveChangesAsync();
            List<Price> prices = new();
            prices.Add(new Price
            {
                IsImportPrice = true,
                Value = detail.ImportPrice,
                IsRetailPrice = false,
                ProductDetailId = newDetail.Id
            });
            prices.Add(new Price
            {
                IsImportPrice = false,
                Value = detail.RetailPrice,
                IsRetailPrice = true,
                ProductDetailId = newDetail.Id
            });
            prices.Add(new Price
            {
                IsImportPrice = false,
                Value = detail.WholePrice,
                IsRetailPrice = false,
                ProductDetailId = newDetail.Id
            });
            await _context.Prices.AddRangeAsync(prices);
            await _context.SaveChangesAsync();
            return Ok(detail);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductDetail([FromRoute] int id, [FromBody] ProductDetailModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var detail = _context.ProductDetails.Include(product => product.Prices).FirstOrDefault() ?? throw new Exception("Not found");
            _context.ProductDetails.Update(detail);
            detail.Unit = model.Unit;
            detail.Description = model.Description;
            detail.IsAvailable = model.IsAvailable;
            detail.ToWholesale = model.ToWholesale;

            if (detail.GetImportPrice() != model.ImportPrice)
            {
                _context.Prices.Add(new Price
                {
                    IsImportPrice = true,
                    Value = model.ImportPrice,
                    IsRetailPrice = false,
                    ProductDetailId = id
                });
            }

            if (detail.GetRetailPrice() != model.RetailPrice)
            {
                _context.Prices.Add(new Price
                {
                    IsImportPrice = false,
                    Value = model.RetailPrice,
                    IsRetailPrice = true,
                    ProductDetailId = id
                });
            }

            if (detail.GetWholePrice() != model.WholePrice)
            {
                _context.Prices.Add(new Price
                {
                    IsImportPrice = false,
                    Value = model.WholePrice,
                    IsRetailPrice = false,
                    ProductDetailId = id
                });
            }



            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductDetail([FromRoute] int id)
        {
            var productDetailToDelete = await _context.ProductDetails.FindAsync(id) ?? throw new Exception("Not found");
            //_context.ProductDetails.Remove(productDetailToDelete);
            productDetailToDelete.IsAvailable = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("statistic/hot")]
        public async Task<IActionResult> GetHotProducts([FromQuery] string criteria = "value", [FromQuery] string time = "month")
        {
            var orders = _context.Orders.Where(order => order.IsSuccess && order.IsProcessed);
            orders = time switch
            {
                "week" => orders.Where(order => order.DateCreate.AddHours(7) >= DateTime.Now.AddDays(-7)),
                _ => orders.Where(order => order.DateCreate.AddHours(7) >= DateTime.Now.AddDays(-30))
            };
            var tableJoined = orders.Join(_context.Carts, order => order.Id, cart => cart.OrderId, (order, cart) => new { order, cart })
                .Join(_context.ProductDetails, orderCart => orderCart.cart.ProductDetailId, detail => detail.Id, (orderCart, detail) => new { orderCart, detail })
                .Join(_context.Products, orderCartDetail => orderCartDetail.detail.ProductId, product => product.Id, (orderCartDetail, product) => new { orderCartDetail, product });

            var tableGrouped = tableJoined.GroupBy(grp => grp.orderCartDetail.detail.Id).Select(grouped => new
            {
                id = grouped.Key,
                detailName = grouped.FirstOrDefault()!.orderCartDetail.detail.Unit,
                productName = grouped.FirstOrDefault()!.product.Name,
                total = grouped.Count(grp => grp.orderCartDetail.orderCart.cart.OrderId != null),
                value = grouped.Sum(grp => grp.orderCartDetail.orderCart.order.Total)
            });

            var result = criteria switch
            {
                "invoice" => tableGrouped.OrderByDescending(grp => grp.total),
                _ => tableGrouped.OrderByDescending(grp => grp.value)
            };
            return Ok(await result.Take(10).ToListAsync());
        }

        [HttpGet("statistic/sell_slowly")]
        public async Task<IActionResult> GetSellSlowlyProduct([FromQuery] string criteria = "value", [FromQuery] string time = "month")
        {
            var products = await _context.ProductDetails
                .Include(product => product.InvoiceDetails)!.ThenInclude(invoice => invoice.Invoice)
                .AsSplitQuery()
                .Include(product => product.Carts)
                .Include(product => product.Product)
                .Include(product => product.Stocks).ToListAsync();
            var result = products.Where(product => product.Stocks!.Count > 0).Select(product =>
            {
                var lastedInvoice = product.InvoiceDetails!.OrderByDescending(invoice => invoice.Invoice!.DateCreate).FirstOrDefault()!.GetDateCreate();
                return new
                {
                    product = _mapper.Map<ProductDetailInfo>(product),
                    current = product.GetStock(),
                    lastedInvoice,
                    wastage = product.Stocks?.Where(stock => stock.DateUpdate.Date >= lastedInvoice.Date && stock.IsManualUpdate).Sum(stock => stock.ManualValue),
                    quantityPerDay = product.Carts?.Where(cart => cart.OrderId != null).Sum(cart => cart.RealQuantity) / ((DateTime.Now.Date - lastedInvoice.Date).TotalDays + 1),
                };
            });
            //var stocks = time switch
            //{
            //    "week" => products.Where(order => order.DateCreate.AddHours(7) >= DateTime.Now.AddDays(-7)),
            //    _ => products.Where(order => order.DateCreate.AddHours(7) >= DateTime.Now.AddDays(-30))
            //};
            //var tableJoined = products.Join(_context.Stocks, product => product.Id, stock => stock.ProductDetailId, (product, stock) => new { product, stock});
            //var tableGroup = tableJoined.GroupBy(productStock => productStock.product.Id).Select(grp => new
            //{
            //    id = grp.Key,
            //    product = grp.FirstOrDefault()!.product,
            //    value = grp.
            //});
            return Ok(result.OrderBy(grp => grp.quantityPerDay).Take(10).ToList());
        }

        [HttpGet("{id}/prices/{type}")]
        public async Task<IActionResult> GetRetailPrices([FromRoute(Name ="id")]int productDetailId, [FromRoute]string type, [FromQuery]string fromDate = "", [FromQuery] string toDate ="")
        {
            var prices = _context.Prices.Where(price => price.ProductDetailId == productDetailId);
            prices = type switch
            {
                "retail" => prices.Where(price => price.IsRetailPrice && !price.IsImportPrice),
                "import" => prices.Where(price => price.IsImportPrice && !price.IsRetailPrice),
                _ => prices.Where(price => !price.IsRetailPrice && !price.IsImportPrice)
            };
            return Ok(_mapper.Map<IEnumerable<PriceInfo>>(await prices.OrderByDescending(price => price.Id).ToListAsync()));
        }
    }
}
