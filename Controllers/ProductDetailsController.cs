using CT554_API.Models;
using CT554_Entity.Data;
using CT554_Entity.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CT554_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductDetailsController : ControllerBase
    {
        private readonly ILogger<ProductDetailsController> _logger;
        private readonly CT554DbContext _context;

        public ProductDetailsController(ILogger<ProductDetailsController> logger, CT554DbContext context)
        {
            _logger = logger;
            _context = context;
        }


        [HttpGet()]
        public async Task<IActionResult> GetProductDetails([FromQuery]int productId, [FromQuery]bool isActive = true)
        {
            List<ProductDetailModel> result = new();
            var details = await _context.ProductDetails.Where(d => d.ProductId== productId).ToListAsync();
            foreach (var detail in details)
            {
                var importPrice = await _context.Prices.Where(p => p.ProductDetailId== detail.Id && p.IsImportPrice)
                    .OrderByDescending(p => p.DateApply).FirstOrDefaultAsync();
                var retailPrice = await _context.Prices.Where(p => p.ProductDetailId == detail.Id && p.IsRetailPrice)
                    .OrderByDescending(p => p.DateApply).FirstOrDefaultAsync();
                var wholePrice = await _context.Prices.Where(p => p.ProductDetailId == detail.Id && !p.IsRetailPrice)
                    .OrderByDescending(p => p.DateApply).FirstOrDefaultAsync();
                if(detail.IsAvailable == isActive) 
                    result.Add(new ProductDetailModel
                    {
                        Id= detail.Id,
                        ProductId = productId,
                        Unit = detail.Unit,
                        ToWholeSale = detail.ToWholesale,
                        Description = detail.Description,
                        ImportPrice = importPrice!.Value,
                        RetailPrice = retailPrice!.Value,
                        WholePrice = wholePrice!.Value,
                        IsActive = detail.IsAvailable
                    }) ;
            }
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> PostProductDetail([FromBody] ProductDetailModel detail)
        {
            var newDetail = new ProductDetail()
            {
                Unit = detail.Unit,
                Description = detail.Description,
                IsAvailable = detail.IsActive,
                ToWholesale = detail.ToWholeSale,
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
    }
}
