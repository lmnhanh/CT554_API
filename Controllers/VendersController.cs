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
    [ApiController]
    [Authorize(policy: "Admin")]
    public class VendersController : ControllerBase
    {
        private readonly CT554DbContext _context;
        private readonly IMapper _mapper;
        public VendersController(CT554DbContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVender([FromRoute] Guid id)
        {
            var vender = await _context.Venders.Include(vender => vender.Invoices).FirstOrDefaultAsync(vender => vender.Id == id);
            if (vender == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<VenderInfo>(vender));
        }

        [HttpGet]
        public async Task<IActionResult> GetVenders([FromQuery] string name = "", [FromQuery] int page = 1,
            [FromQuery] int size = 5, [FromQuery] string sort = "", [FromQuery] string order = "")
        {
            HashSet<Vender> venders = new();
            var temp_list = await _context.Venders.Include(v => v.Invoices).ToListAsync();
            if (page <= 0)
            {
                return Ok(
                   new { 
                       venders = _mapper.Map<List<VenderInfo>>(await _context.Venders.ToListAsync()).OrderByDescending(v => v.InvoiceCount) 
                   });
            }

            if (name != "")
            {
                var keys = name.Split(' ');
                foreach (var key in keys)
                {
                    venders.AddRange(temp_list.Where(c => c.Name.ToLower().Split(' ').Contains(key.ToLower())));
                }
            }
            else
            {
                venders = temp_list.ToHashSet();
            }

            var totalRows = venders.Count;

            venders = order.ToLower() switch
            {
                "desc" => sort.ToLower() switch
                {
                    "name" => venders.OrderByDescending(c => c.Name).ToHashSet(),
                    "datestart" => venders.OrderByDescending(c => c.DateStart).ToHashSet(),
                    "invoice" => venders.OrderByDescending(c => c.Invoices?.Count).ToHashSet(),
                    _ => venders.OrderByDescending(c => c.Id).ToHashSet()
                },
                _ => sort.ToLower() switch
                {
                    "name" => venders = venders.OrderBy(c => c.Name).ToHashSet(),
                    "datestart" => venders = venders.OrderBy(c => c.DateStart).ToHashSet(),
                    "invoice" => venders.OrderBy(c => c.Invoices?.Count).ToHashSet(),
                    _ => venders = venders.OrderBy(c => c.Id).ToHashSet()
                }
            };

            return Ok(
                new
                {
                    venders = _mapper.Map<IEnumerable<VenderInfo>>(venders.Skip((page - 1) * size).Take(size)),
                    totalRows,
                    totalPages = (totalRows - 1) / size + 1,
                });
        }

        [HttpPost]
        public async Task<IActionResult> PostVender([FromBody] Vender vender)
        {
            _context.Venders.Add(vender);
            await _context.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created, vender);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutVender([FromRoute] Guid id, Vender vender)
        {
            if (id != vender.Id)
            {
                return BadRequest();
            }
            _context.Venders.Entry(vender).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVender([FromRoute] Guid id)
        {
            var vender = _context.Venders.Find(id);
            if (vender == null)
            {
                return BadRequest();
            }
            _context.Venders.Remove(vender);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
