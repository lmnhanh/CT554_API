using AutoMapper;
using CT554_API.Models.Statistic;
using CT554_Entity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CT554_API.Controllers
{
	[Route("api/[controller]")]
	[Authorize(Policy = "Admin")]
	[ApiController]
	public class StatisticController : ControllerBase
	{
		private readonly CT554DbContext _context;
		private IMapper _mapper;
		public StatisticController(CT554DbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		[HttpGet("profit")]
		public async Task<IActionResult> GetStatisticProfit([FromQuery] string fromDate = "", [FromQuery] string toDate = "")
		{
			var orders = _context.Orders.Where(order => order.IsSuccess && order.IsProcessed);
			var invoices = _context.Invoices.AsQueryable();

			var fromDateParsed = fromDate == "" ? DateTime.Now.AddDays(-7).Date : DateTime.Parse(fromDate).Date;
			var toDateParsed = toDate == "" ? DateTime.Now.Date : DateTime.Parse(toDate).Date;

			if (toDateParsed - fromDateParsed > TimeSpan.FromDays(90))
			{
				return BadRequest();
			}

			orders = orders.Where(order => order.DateSuccess != null &&
				order.DateSuccess.Value.AddHours(7).Date.CompareTo(fromDateParsed) >= 0 &&
				order.DateSuccess.Value.AddHours(7).Date.CompareTo(toDateParsed) <= 0
			);

			invoices = invoices.Where(invoice =>
				invoice.DateCreate.AddHours(7).Date.CompareTo(fromDateParsed) >= 0 &&
				invoice.DateCreate.AddHours(7).Date.CompareTo(toDateParsed) <= 0
			);

			var ordersToResult = await orders.ToListAsync();
			var invoicesToResult = await invoices.ToListAsync();

			List<RevenueCostModel> chartModels = new();
			var dateSpan = (toDateParsed.Date - fromDateParsed.Date);
			if (dateSpan.Days > 60)
			{
				return BadRequest();
			}

			for (int i = 0; i <= dateSpan.Days; i++)
			{
				chartModels.Add(new RevenueCostModel { Date = fromDateParsed.Date.AddDays(i) });
			}

			foreach (var data in chartModels)
			{
				data.Revenue = ordersToResult.Where(order => order.DateSuccess!.Value.AddHours(7).Date == data.Date.Date).Sum(order => order.Total);
				data.Cost = invoicesToResult.Where(invoice => invoice.DateCreate.AddHours(7).Date == data.Date.Date).Sum(invoice => invoice.RealTotal);
				data.Profit = data.Revenue - data.Cost;
			}

			//invoices = _mapper.Map<IEnumerable<InvoiceInfo>>(invoicesToResult),
			//    orders = _mapper.Map<IEnumerable<OrderInfo>>(ordersToResult),

			return Ok(new
			{
				chartData = chartModels,
				fromDate = fromDateParsed,
				toDate = toDateParsed,
				profit = ordersToResult.Sum(order => order.Total) - invoicesToResult.Sum(invoice => invoice.RealTotal)
			});
		}

		[HttpGet("prices/{id}")]
		public async Task<IActionResult> GetPricesHistory([FromRoute] int id, [FromQuery] string fromDate = "", [FromQuery] string toDate = "")
		{
			var query = _context.Prices.Where(product => product.ProductDetailId == id);


			var fromDateParsed = fromDate == "" ? DateTime.Now.AddMonths(-1) : DateTime.Parse(fromDate);
			query = query.Where(price => price.DateApply.AddHours(7).Date.CompareTo(fromDateParsed.Date) >= 0);

			var toDateParsed = toDate == "" ? DateTime.Now : DateTime.Parse(toDate);
			Console.WriteLine(toDate);
			query = query.Where(price => price.DateApply.AddHours(7).Date.CompareTo(toDateParsed.Date) <= 0);

			var pricesGrouped = await query.GroupBy(price => price.DateApply.AddHours(7).Date).ToListAsync();
			var allPrices = await query.ToListAsync();
			List<Prices> prices = pricesGrouped.Select(grp => new Prices { Date = grp.Key }).ToList();
			if (prices.Count > 90) return BadRequest();
			foreach (var price in prices)
			{
				var group = pricesGrouped.FirstOrDefault(grp => grp.Key == price.Date);
				var temp = group!.Where(price => price.IsRetailPrice);
				if (!temp.Any())
				{
					price.RetailValue = allPrices.Where(price => price.IsRetailPrice).OrderByDescending(price => price.DateApply).FirstOrDefault()!.Value;
				}
				else
				{
					price.RetailValue = temp.Average(price => price.Value);
				}
				temp = group!.Where(price => price.IsImportPrice);
				if (!temp.Any())
				{
					price.ImportValue = allPrices.Where(price => price.IsImportPrice).OrderByDescending(price => price.DateApply).FirstOrDefault()!.Value;
				}
				else
				{
					price.ImportValue = temp.Average(price => price.Value);
				}
				temp = group!.Where(price => !price.IsImportPrice && !price.IsRetailPrice);
				if (!temp.Any())
				{
					price.WholeValue = allPrices.Where(price => !price.IsImportPrice && !price.IsRetailPrice).OrderByDescending(price => price.DateApply).FirstOrDefault()!.Value;
				}
				else
				{
					price.WholeValue = temp.Average(price => price.Value);
				}
			}
			return Ok(new
			{
				prices,
				fromDate = prices.FirstOrDefault()?.Date ?? DateTime.Now,
				toDate = prices.LastOrDefault()?.Date ?? DateTime.Now
			});
		}
	}
}
