using CT554_API.Models.Common;
using CT554_Entity.Data;
using CT554_Entity.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CT554_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(policy: "Admin")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly CT554DbContext _context;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(CT554DbContext context, ILogger<ImagesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Images
        [HttpGet]
        public async Task<ActionResult<List<string>>> GetImages([FromQuery]int productId = 0)
        {
            if (_context.Images == null)
            {
                return NotFound();
            }
            var images = productId switch
            {
                0 => _context.Images.Select(image => image.URL).ToListAsync(),
                _ => _context.Images.Where(image => image.ProductId == productId).Select(image => image.URL).ToListAsync()
            };
            return await images;
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

        [HttpGet("ofProduct/{id}")]
        public async Task<ActionResult<List<ImageResponse>>> GetImagesOfProduct(int id)
        {
            if (_context.Images == null) 
            {
                return NotFound();
            }
            var images = await _context.Images.Where(image => image.ProductId == id).ToListAsync();

            if (images.Count == 0)
            {
                return NotFound();
            }
            return images.Select(image => new ImageResponse() { Data = image.Content, Name = image.URL}).ToList();
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

        [HttpGet("get/{id}"), AllowAnonymous]
        public IActionResult getProductImage(string id)
        {
           var image = _context.Images.Find(id);
            //return "data:image/png;base64," + Convert.ToBase64String(content);
            if (image is null) return BadRequest();
           return File(image.Content, "image/png");
        }
    }
}
