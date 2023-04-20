using AutoMapper;
using CT554_API.Models;
using CT554_Entity.Data;
using CT554_Entity.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace CT554_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(policy: "Admin")]
    //[Authorize(Roles ="Admin")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly CT554DbContext _context;
        private readonly IMapper _mapper;

        public ProductsController(CT554DbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Products
        //recommended: -1: all, 0: false, 1: true
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] int? categoryId, [FromQuery] bool? hasPromotion, [FromQuery] string name = "", [FromQuery] string filter = "", [FromQuery] int page = 1,
            [FromQuery] int size = 5, [FromQuery] string sort = "", [FromQuery] string order = "")
        {
            if (_context.Products == null)
            {
                return NotFound();
            }

            HashSet<Product> products = new();
            var temp_list = categoryId switch
            {
                int id => await _context.Products.Where(product => product.CategoryId == id).Include(product => product.Category).Include(product => product.Details).ToListAsync(),
                _ => await _context.Products.Include(product => product.Category).Include(product => product.Details).ToListAsync()
            };

            if (page <= 0)
            {
                var result = _context.Products.AsQueryable();
                if(hasPromotion.HasValue && !hasPromotion.Value)
                {
                    result = result.Where(product => !product.Promotions!.Any(promotion => promotion.IsActive || promotion.DateEnd.AddHours(7) >= DateTime.Now));
                }
                return Ok(new { products = result.Where(product => product.IsActive).Select(product => new { id = product.Id, name = product.Name }).ToList() });
            }

            if (name != "")
            {
                var keys = name.Split(' ');
                foreach (var key in keys)
                {
                    products.AddRange(temp_list.Where(c => c.Name.ToLower().Split(' ').Contains(key.ToLower())));
                }
            }
            else
            {
                products = temp_list.ToHashSet();
            }

            if (filter != string.Empty)
            {
                products = filter.ToLower() switch
                {
                    "active" => products.Where(c => c.IsActive).ToHashSet(),
                    "recommended" => products.Where(c => c.IsRecommended).ToHashSet(),
                    _ => products.Where(c => !c.IsActive).ToHashSet(),
                };

            }

            var totalRows = products.Count;

            products = order.ToLower() switch
            {
                "desc" => sort.ToLower() switch {
                    "name" => products.OrderByDescending(c => c.Name).ToHashSet(),
                    "dateupdate" => products.OrderByDescending(c => c.DateUpdate).ToHashSet(),
                    _ => products.OrderByDescending(c => c.Id).ToHashSet()
                },
                _ => sort.ToLower() switch {
                    "name" => products = products.OrderBy(c => c.Name).ToHashSet(),
                    "dateupdate" => products.OrderBy(c => c.DateUpdate).ToHashSet(),
                    _ => products = products.OrderBy(c => c.Id).ToHashSet()
                }
            };

            return Ok(new
            {
                products = _mapper.Map<IEnumerable<ProductInfo>>(products.Skip((page - 1) * size).Take(size)),
                totalRows,
                totalPages = (totalRows - 1) / size + 1,
            });
        }

        [AllowAnonymous]
        [HttpPost("UploadImage")]
        public async Task<ActionResult> PostFiles([FromForm] IFormFile images, [FromHeader(Name = "productId")] int productId)
        {
            if (images == null)
            {
                return BadRequest();
            }

            try
            {
                //foreach (IFormFile file in images)
                //{
                var image = new Image();
                var extension = Path.GetExtension(images.FileName);
                var acceptExtension = new[] { ".jpg", ".png", ".jpeg" };
                if (acceptExtension.Contains(extension))
                {
                    var id = $"{images.Length}_{images.GetHashCode()}{extension}";
                    using (var stream = new MemoryStream())
                    {
                        images.CopyTo(stream);
                        image.URL = id;
                        image.ProductId = productId;
                        image.Content = stream.ToArray();
                    }

                    await _context.Images.AddAsync(image);
                    //}
                    await _context.SaveChangesAsync();
                    return Ok(id);
                }
                return BadRequest();
            }
            catch (Exception)
            {
                throw;
            }
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.Include(product => product.Category)
                .Include(product => product.Details)!.ThenInclude(detail => detail.Prices)
                .AsSplitQuery()
                .Include(product => product.Details)!.ThenInclude(detail => detail.Stocks)
                .FirstOrDefaultAsync(product => product.Id == id);
            foreach (var claim in User.Claims.ToList())
            {
                Console.Write(claim.Type + " " + claim.Value);
            }
            if (product == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ProductInfo>(product));
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductDTO model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }
            var product = _context.Products.Find(id) ?? throw new Exception("Product is not found");
            product.DateUpdate= DateTime.UtcNow;
            try
            {
                var errors = new List<object>();
                if (_context.Products.Any(c => c.WellKnownId.ToLower() == model.WellKnownId.ToLower() && c.Id != id))
                    errors.Add(new { wellKnownId = "Mã sản phẩm đã tồn tại" });
                if (_context.Products.Any(c => c.Name.ToLower() == model.Name.ToLower() && c.Id != id))
                    errors.Add(new { name = "Tên sản phẩm đã tồn tại" });
                if (errors.Count != 0)
                    return BadRequest(errors);
                product.Name = model.Name;
                product.WellKnownId = model.WellKnownId;
                product.Description= model.Description;
                product.IsActive= model.IsActive;
                product.IsRecommended = model.IsRecommended;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([Bind("Id,Name,Description,WellKnownId,CategoryId,IsActive,IsRecommended")] ProductDTO product)
        {

            var errors = new List<object>();
            if (_context.Products.Any(c => c.WellKnownId.ToLower() == product.WellKnownId.ToLower()))
                errors.Add(new { wellKnownId = "Mã sản phẩm đã tồn tại" });
            if (_context.Products.Any(c => c.Name.ToLower() == product.Name.ToLower()))
                errors.Add(new { name = "Tên sản phẩm đã tồn tại" });
            if (errors.Count != 0)
                return BadRequest(errors);

            var productToAdd = _mapper.Map<Product>(product);
			_context.Products.Add(productToAdd);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, productToAdd);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
