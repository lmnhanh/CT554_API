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
    [Authorize(policy: "Admin")]
    //[Authorize(Roles ="Admin")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly CT554DbContext _context;
        private readonly IMapper _mapper;

        public OrdersController(CT554DbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder([FromRoute] string id)
        {
            var orderHasFound = await _context.Orders.Include(order => order.Carts)!
                .ThenInclude(cart => cart.ProductDetail).ThenInclude(detail => detail!.Prices)
                .AsSplitQuery()
                .Include(order => order.Carts)!
                .ThenInclude(cart => cart.ProductDetail).ThenInclude(detail => detail!.Product)
                .Include(order => order.User)
                .FirstOrDefaultAsync(order => order.Id == new Guid(id)) ?? throw new Exception("Order was not found");

            foreach (var cart in orderHasFound.Carts!)
            {
                cart.ProductDetail!.TargetDate = orderHasFound.DateCreate;
            }

            return Ok(_mapper.Map<OrderInfo>(orderHasFound));
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] string filter = "", [FromQuery] string userName = "", [FromQuery] int productId = 0, [FromQuery] string fromDate = "",
            [FromQuery] string toDate = "", [FromQuery] int page = 1,
            [FromQuery] float fromPrice = -1, [FromQuery] float toPrice = -1,
            [FromQuery] int size = 10, [FromQuery] string sort = "", [FromQuery] string order = "")
        {
            var query = _context.Orders.AsQueryable();

            if (filter != "")
            {
                query = filter switch
                {
                    "processed" => query.Where(order => order.IsProccesed && !order.IsSuccess),
                    "processing" => query.Where(order => !order.IsProccesed && !order.IsSuccess),
                    "success" => query.Where(order => order.IsProccesed && order.IsSuccess),
                    "fail" => query.Where(order => !order.IsProccesed && order.IsSuccess),
                    _ => query.Where(order => order.User == null)
                };
            }

            if (userName != "")
            {
                var searchKey = userName.ToLower().Split(new char[] { ' ' });
                query = query.Where(c => c.User != null && c.User.FullName.ToLower().Split(new char[] { ' ' }).Any(key => searchKey.Contains(key)));
                if (productId != 0)
                {
                    query = query.Where(c => c.Carts!.Any(detail => detail.ProductDetail!.ProductId == productId));
                }
            }
            else
            {
                if (productId != 0)
                {
                    query = query.Where(c => c.Carts!.Any(detail => detail.ProductDetail!.ProductId == productId));
                }
            }

            if (fromPrice != -1)
            {
                query = query.Where(c => c.Total >= fromPrice);
                if (toPrice != -1)
                {
                    query = query.Where(c => c.Total <= toPrice);
                }
            }
            else
            {
                if (toPrice != -1)
                {
                    query = query.Where(c => c.Total <= toPrice);
                }
            }

            if (fromDate != "")
            {
                var toDateParsed = DateTime.Parse(toDate);
                var fromDateParsed = toDate == "" ? DateTime.UtcNow : DateTime.Parse(fromDate);

                query = query.Where(order =>
                    order.DateCreate.Date.CompareTo(DateTime.Parse(fromDate).Date) >= 0 &&
                    order.DateCreate.Date.CompareTo(DateTime.Parse(toDate).Date) <= 0
                );

            }


            query = order.ToLower() switch
            {
                "desc" => sort.ToLower() switch
                {
                    "total" => query.OrderByDescending(c => c.Total),
                    "datecreate" => query.OrderByDescending(c => c.DateCreate),
                    _ => query.OrderByDescending(c => c.Id)
                },
                _ => sort.ToLower() switch
                {
                    "total" => query = query.OrderBy(c => c.Total),
                    "datecreate" => query = query.OrderBy(c => c.DateCreate),
                    _ => query = query.OrderBy(c => c.Id)
                }
            };

            var orders = await query.Include(order => order.Carts)!.ThenInclude(cart => cart.ProductDetail)
                .Include(order => order.User).Skip((page - 1) * size).Take(size).ToListAsync();
            var totalRows = await query.CountAsync();
            return Ok(
                new
                {
                    orders = _mapper.Map<IEnumerable<OrderInfo>>(orders),
                    totalRows,
                    totalPages = (totalRows - 1) / size + 1,
                });
        }

        [HttpPost("cart")]
        public async Task<IActionResult> PostCart([FromBody] CartDTO cart)
        {
            var cartToAdd = _mapper.Map<Cart>(cart);
            await _context.Carts.AddAsync(cartToAdd);
            await _context.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created, cartToAdd);
        }

        [HttpPost]
        public async Task<IActionResult> PostOrder([FromBody] OrderDTO orderModel)
        {
            var orderToAdd = _mapper.Map<Order>(orderModel);
            await _context.Orders.AddAsync(orderToAdd);

            float total = 0;

            foreach (var cart in orderModel.Carts)
            {
                var productDetail = await _context.ProductDetails.Include(product => product.Stocks).Include(product => product.Prices).FirstOrDefaultAsync(product => product.Id == cart.ProductDetailId)
                    ?? throw new Exception("Not found");
                if (!productDetail.IsAvailable) return BadRequest();

                if (orderToAdd.UserId != null)
                {
                    total += productDetail.GetWholePrice() * cart.Quantity;
                }
                else
                {
                    total += (productDetail.ToWholesale <= cart.Quantity ? productDetail.GetWholePrice() : productDetail.GetRetailPrice()) * cart.Quantity;
                }
            }


            if (orderToAdd.Total == 0)
            {
                orderToAdd.Total = total;
            }
            await _context.SaveChangesAsync(true);

            return StatusCode(StatusCodes.Status201Created, _mapper.Map<OrderInfo>(orderToAdd));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(string id, OrderDTO orderModel)
        {
            if (id != orderModel.Id)
            {
                return BadRequest();
            }
            float total = 0;
            var orderToUpdate = _mapper.Map<Order>(orderModel);
            foreach(var cart in orderToUpdate.Carts!)
            {
                var productDetail = await _context.ProductDetails.Include(product => product.Stocks).Include(product => product.Prices).FirstOrDefaultAsync(product => product.Id == cart.ProductDetailId)
                    ?? throw new Exception("Not found");
                if (!productDetail.IsAvailable) return BadRequest();

                if (orderToUpdate.UserId != null)
                {
                    total += productDetail.GetWholePrice() * cart.RealQuantity;
                }
                else
                {
                    total += (productDetail.ToWholesale <= cart.RealQuantity ? productDetail.GetWholePrice() : productDetail.GetRetailPrice()) * cart.RealQuantity;
                }

                var currentNotManualStock = productDetail.Stocks?.Where(stock => !stock.IsManualUpdate && stock.DateUpdate.Date.CompareTo(orderToUpdate.DateCreate.Date) == 0).FirstOrDefault();
                if (currentNotManualStock == null)
                {
                    var currentStock = productDetail.Stocks?.OrderByDescending(stock => stock.DateUpdate).FirstOrDefault() ?? throw new Exception("Not found stock");
                    await _context.Stocks.AddAsync(new Stock { ProductDetailId = cart.ProductDetailId, Value = currentStock.Value - cart.RealQuantity });
                }
                else
                {
                    _context.Stocks.Update(currentNotManualStock);
                    currentNotManualStock.Value -= cart.RealQuantity;
                }
            }
            _context.Orders.Update(orderToUpdate);
            orderToUpdate.Total = total;
            orderToUpdate.IsProccesed = true;
            orderToUpdate.DateProccesed = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(_mapper.Map<OrderInfo>(orderToUpdate));
        }
    }
}
