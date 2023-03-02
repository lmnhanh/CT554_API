using CT554_API.Data;
using CT554_API.Entity;
using CT554_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace CT554_API.Controllers
{
    [Route("api/[controller]")]
    //[Authorize(Policy ="Admin")]
    [Authorize(Roles ="Admin")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly CT554DbContext _context;

        public ProductsController(CT554DbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<object>> GetProducts([FromQuery] string name = "", [FromQuery] string filter = "", [FromQuery] int page = 1,
            [FromQuery] int size = 5, [FromQuery] string sort = "", [FromQuery] string order = "")
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            HashSet<Product> products = new();
            var temp_list = await _context.Products.Include(p => p.Category).ToListAsync();
            if (name != "")
            {
                var keys = name.Split(' ');
                foreach (var key in keys)
                {
                   products.AddRange(temp_list.Where(c => c.Name.ToLower().Contains(key.ToLower())));
                }
            }
            else
            {
               products = temp_list.ToHashSet();
            }
            products = filter switch
            {
                "active" => products.Where(c => c.IsActive).ToHashSet(),
                "unactive" => products.Where(c => !c.IsActive).ToHashSet(),
                _ => products,
            };
            var totalRows = products.Count;

            products = products.OrderBy(c => c.Id).ToHashSet();
            if (order.ToLower().Contains("desc"))
            {
                products = products.OrderByDescending(c => c.Id).ToHashSet();
                if (sort.ToLower().Contains("name"))
                {
                    products = products.OrderByDescending(c => c.Name).ToHashSet();
                }
                if (sort.ToLower().Contains("dateupdate"))
                {
                    products = products.OrderByDescending(c => c.DateUpdate).ToHashSet();
                }
            }
            else
            {
                if (sort.ToLower().Contains("name"))
                {
                    products = products.OrderBy(c => c.Name).ToHashSet();
                }
                if (sort.ToLower().Contains("dateupdate"))
                {

                    products = products.OrderBy(c => c.DateUpdate).ToHashSet();
                }
            }

            return new
            {
                products = products.Skip((page - 1) * size).Take(size),
                totalRows,
                totalPages = (totalRows - 1) / size + 1,
            };
        }

        [AllowAnonymous]
        [HttpPost("UploadImage")]
        public async Task<ActionResult> PostFiles([FromForm] IFormFile images)
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
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            foreach(var claim in User.Claims.ToList())
            {
                Console.Write(claim.Type + " " + claim.Value);
            }
            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
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
        public async Task<ActionResult<Product>> PostProduct([Bind("Id,Name,Description,WellKnownId,CategoryId,IsActive,IsRecommended")]Product product)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'CT554DbContext.Products'  is null.");
            }
            //if (_context.Products.Any(c => c.Name.ToLower() == product.Name.ToLower()))
            //    return BadRequest(new ErrorResponse
            //    {
            //        errors = new List<string> {"Tên loại hải sản đã tồn tại."
            //    }
            //    });
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
