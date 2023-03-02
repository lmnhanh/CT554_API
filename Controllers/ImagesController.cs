using CT554_API.Data;
using CT554_API.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CT554_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(policy: "Admin")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly CT554DbContext _context;

        public ImagesController(CT554DbContext context)
        {
            _context = context;
        }

        // GET: api/Images
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Image>>> GetImages()
        {
            if (_context.Images == null)
            {
                return NotFound();
            }
            return await _context.Images.ToListAsync();
        }

        // GET: api/Images/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetImage(string id)
        {
            if (_context.Images == null)
            {
                return NotFound();
            }
            var image = await _context.Images.FindAsync(id);

            if (image == null)
            {
                return NotFound();
            }
            Response.Headers.Add("Content-Disposition", $"inline; filename=\"{image.URL}\"");
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

            //Response.Headers.Add("filename", image.URL);
            return new { name= image.URL, data = image.Content };
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(string id)
        {
            if (_context.Images == null)
            {
                return NotFound();
            }
            var image = await _context.Images.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
