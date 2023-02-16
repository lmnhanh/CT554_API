using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CT554_API.Data;
using CT554_API.Entity;
using CT554_API.Models;
using Microsoft.AspNetCore.Authorization;

namespace CT554_API.Controllers
{
    [Route("api/[controller]")]
    //[Authorize(policy: "Admin")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly CT554DbContext _context;

        public CategoriesController(CT554DbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<object>> GetCategories([FromQuery]string filter = "", [FromQuery]int page=1,
            [FromQuery] int size = 5, [FromQuery] string sort = "", [FromQuery] string order = "")
        {
            if (_context.Categories == null)
            {
                return NotFound();
            }

            List<Category> categories = new List<Category>();
			categories = filter switch
			{
				"active" => await _context.Categories.Where(c => c.IsActive).ToListAsync(),
				"unactive" => await _context.Categories.Where(c => !c.IsActive).ToListAsync(),
				_ => await _context.Categories.ToListAsync(),
			};
            var totalRows = categories.Count();

			categories = categories.OrderBy(c => c.Id).ToList();
			if (order.ToLower().Contains("desc"))
			{
				categories = categories.OrderByDescending(c => c.Id).ToList();
				if (sort.ToLower().Contains("name"))
				{
				    categories = categories.OrderByDescending(c => c.Name).ToList();
				}
				if (sort.ToLower().Contains("dateupdate"))
				{
					categories = categories.OrderByDescending(c => c.DateUpdate).ToList();
				}
            }
            else
            {
				if (sort.ToLower().Contains("name"))
				{
					categories = categories.OrderBy(c => c.Name).ToList();
				}
				if (sort.ToLower().Contains("dateupdate"))
				{
					
					categories = categories.OrderBy(c => c.DateUpdate).ToList();
				}
			}
            
			return new { 
                categories = categories.Skip((page - 1) * size).Take(size),
                totalRows,
                totalPages = (totalRows-1)/ size + 1,
			}; 
        }

        [HttpGet("/overall")]
        public async Task<object> GetOverallStatistic()
        {
            return new
            {
                total = await _context.Categories.CountAsync(),
                totalActive = await _context.Categories.Where(c => c.IsActive).CountAsync(),
                totalUnactive = await _context.Categories.Where(c => !c.IsActive).CountAsync()
            };
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
          if (_context.Categories == null)
          {
              return NotFound();
          }
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Categories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, [FromBody]Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
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

        // POST: api/Categories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
          if (_context.Categories == null)
          {
              return Problem("Entity set 'CT554DbContext.Categories'  is null.");
          }
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (_context.Categories == null)
            {
                return NotFound();
            }
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
