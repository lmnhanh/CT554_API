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
	[Authorize(Policy = "Admin")]
	[ApiController]
	public class PromotionsController : ControllerBase
	{
		private readonly CT554DbContext _context;
		private readonly IMapper _mapper;

		public PromotionsController(CT554DbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		[HttpPost]
		public async Task<IActionResult> PostPromotion([FromBody] PromotionDTO promotion)
		{
			var promotionToAdd = _mapper.Map<Promotion>(promotion);
			promotionToAdd.DateCreate = DateTime.UtcNow;
			promotionToAdd.Products = _context.Products.Where(product => promotion.ProductIds!.Contains(product.Id)).ToList();
			await _context.Promotions.AddAsync(promotionToAdd);
			await _context.SaveChangesAsync();
			return StatusCode(StatusCodes.Status201Created, _mapper.Map<PromotionInfo>(promotionToAdd));
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetPromotion([FromRoute]string id)
		{
			var promotionToResult = await _context.Promotions.Include(promotion => promotion.Products)!.ThenInclude(product => product.Category)
				.FirstOrDefaultAsync(promotion => promotion.Id == Guid.Parse(id));
			return Ok(_mapper.Map<PromotionInfo>(promotionToResult));
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeletePromotion([FromRoute] string id)
		{
			var promotionToDelete = _context.Promotions.Find(Guid.Parse(id)) ?? throw new Exception("Promotion is not found");
			_context.Promotions.Remove(promotionToDelete);
			await _context.SaveChangesAsync();
			return NoContent();
		}
		[HttpPut("{id}")]
		public async Task<IActionResult> PutPromotion([FromRoute] string id)
		{
			var promotionToUpdate = _context.Promotions.Find(Guid.Parse(id)) ?? throw new Exception("Promotion is not found");
			promotionToUpdate.IsActive = false;
			_context.Promotions.Update(promotionToUpdate);
			await _context.SaveChangesAsync();
			return NoContent();
		}


		[HttpGet]
		public async Task<IActionResult> GetPromotions([FromQuery] string filter = "", [FromQuery] string name = "",
			[FromQuery] int productId = 0, [FromQuery] string fromDate = "",
			[FromQuery] string toDate = "", [FromQuery] int page = 1, [FromQuery] string type = "",
			[FromQuery] int size = 10, [FromQuery] string sort = "", [FromQuery] string order = "")
		{
			var query = _context.Promotions.AsQueryable();

			if (filter != "")
			{
				query = filter switch
				{
					"takingplace" => query.Where(promotion => promotion.DateStart.AddHours(7).CompareTo(DateTime.Now) <= 0
													&& promotion.DateEnd.AddHours(7).CompareTo(DateTime.Now) >= 0),
					"incoming" => query.Where(promotion => promotion.DateStart.AddHours(7).CompareTo(DateTime.Now) > 0),
					"passed" => query.Where(promotion => promotion.DateEnd.AddHours(7).CompareTo(DateTime.Now) < 0),
					"unavailable" => query.Where(promotion => !promotion.IsActive),
					_ => query
				};
			}

			if (type != "")
			{
				query = type.ToLower() switch
				{
					"percent" => query.Where(promotion => promotion.IsPercentage),
					_ => query.Where(promotion => !promotion.IsPercentage),
				};
			}

			if (name != "")
			{
				query = query.Where(promotion => promotion.Name.ToLower().Contains(name.ToLower()));
			}

			if (fromDate != "")
			{
				var fromDateParsed = DateTime.Parse(fromDate);

				query = query.Where(promotion => promotion.DateStart.AddHours(7).Date.CompareTo(fromDateParsed.Date) >= 0);
			}

			if (toDate != "")
			{
				var toDateParsed = DateTime.Parse(toDate);

				query = query.Where(promotion => promotion.DateStart.AddHours(7).Date.CompareTo(toDateParsed.Date) <= 0);
			}

			var promotions = await query.Include(promotion => promotion.Products).Skip((page - 1) * size).Take(size).ToListAsync();
			if (productId != 0)
			{
				promotions = promotions.Where(promotion => promotion.Products != null && promotion.Products.Any(product => product.Id == productId)).ToList();
			}
			var totalRows = promotions.Count;
			promotions = order.ToLower() switch
			{
				"desc" => sort.ToLower() switch
				{
					"datecreate" => promotions.OrderByDescending(c => c.DateCreate).ToList(),
					"product" => promotions.OrderByDescending(c => c.Products!.Count).ToList(),
					_ => promotions.OrderByDescending(c => c.Id).ToList()
				},
				_ => sort.ToLower() switch
				{
					"datecreate" => promotions.OrderBy(c => c.DateCreate).ToList(),
					"product" => promotions.OrderByDescending(c => c.Products!.Count).ToList(),
					_ => promotions.OrderBy(c => c.Id).ToList()
				}
			};

			return Ok(
				new
				{
					promotions = _mapper.Map<IEnumerable<PromotionInfo>>(promotions),
					totalRows,
					totalPages = (totalRows - 1) / size + 1,
				});
		}
	}
}
