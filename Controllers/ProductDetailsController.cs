using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly CT554DbContext _context;

        public ProductDetailsController(ILogger<ProductDetailsController> logger, CT554DbContext context, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
        }


        [HttpGet()]
        public async Task<IActionResult> GetProductDetails([FromQuery] int productId, [FromQuery] bool isActive = true)
        {
            var details = _context.ProductDetails.Where(d => d.ProductId == productId);
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
                IsAvailable = detail.IsActive,
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
            if(id != model.Id)
            {
                return BadRequest();
            }

            var detail = _context.ProductDetails.Find(id) ?? throw new Exception("Not found");
            _context.ProductDetails.Update(detail);
            detail.Unit = model.Unit;
            detail.Description = model.Description;
            detail.IsAvailable = model.IsActive;
            detail.ToWholesale = model.ToWholesale;

            _context.Prices.Add(new Price
            {
                IsImportPrice = true,
                Value = model.ImportPrice,
                IsRetailPrice = false,
                ProductDetailId = id
            });
            _context.Prices.Add(new Price
            {
                IsImportPrice = false,
                Value = model.RetailPrice,
                IsRetailPrice = true,
                ProductDetailId = id
            });
            _context.Prices.Add(new Price
            {
                IsImportPrice = false,
                Value = model.WholePrice,
                IsRetailPrice = false,
                ProductDetailId = id
            });

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductDetail([FromRoute] int id)
        {
            var productDetailToDelete = await _context.ProductDetails.FindAsync(id) ?? throw new Exception("Not found");
            _context.ProductDetails.Remove(productDetailToDelete);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
