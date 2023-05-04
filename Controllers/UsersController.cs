using AutoMapper;
using CT554_API.Models.View;
using CT554_Entity.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CT554_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(policy: "Admin")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        public UsersController(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]string role = "Customer")
        {
            var customers = await _userManager.GetUsersInRoleAsync(role);
            return Ok(customers.Where(user => user.DateAsPartner != null && user.EmailConfirmed).Select(customer => new {id = customer.Id, fullName = customer.FullName}));
        }

        [HttpGet("with_info")]
        public async Task<IActionResult> GetUsersWithInfo([FromQuery] string name = "", [FromQuery] string status = "", [FromQuery] string sort = "",
            [FromQuery] string order = "desc", [FromQuery]string fromDate = "", [FromQuery] string toDate = "",
            [FromQuery] int page = 1, [FromQuery]int size = 10, [FromQuery]string filter = "")
        {
            var users = await _userManager.GetUsersInRoleAsync("Customer");

            if (filter != "")
            {
                users = filter switch
                {
                    "pending" => users.Where(user => user.DateAsPartner == null).ToList(),
                    "emailConfirmed" => users.Where(user => !user.EmailConfirmed).ToList(),
                    _ => users.Where(user => user.DateAsPartner != null && user.EmailConfirmed && user.PhoneNumberConfirmed).ToList()
                };
            }

            if (name != "")
            {
                var searchKey = name.ToLower().Split(new char[] { ' ' });
                users = users.Where(c => c.FullName.ToLower().Split(' ').Any(word => searchKey.Contains(word)) || c.Email!.Substring(0, c.Email.IndexOf('@')).Contains(name)).ToList();
            }

            if (fromDate != "")
            {
                var toDateParsed = toDate == "" ? DateTime.Now : DateTime.Parse(toDate);
                var fromDateParsed = fromDate == "" ? DateTime.UtcNow : DateTime.Parse(fromDate);

                users = users.Where(order =>
                    order.DateCreate.AddHours(7).CompareTo(DateTime.Parse(fromDate)) >= 0 &&
                    order.DateCreate.AddHours(7).CompareTo(DateTime.Parse(toDate)) <= 0
                ).ToList();

            }


            users = order.ToLower() switch
            {
                "desc" => sort.ToLower() switch
                {
                    "dateaspartner" => users.OrderByDescending(c => c.DateAsPartner).ToList(),
                    _ => users.OrderByDescending(c => c.Id).ToList()
                },
                _ => sort.ToLower() switch
                {
                    "dateaspartner" => users = users.OrderBy(c => c.DateAsPartner).ToList(),
                    _ => users = users.OrderBy(c => c.Id).ToList()
                }
            };

            var totalRows = users.Count();
            users = users.Skip((page - 1) * size).Take(size).ToList();
            return Ok(
                new
                {
                    partners = _mapper.Map<IEnumerable<UserInfo>>(users),
                    totalRows,
                    totalPages = (totalRows - 1) / size + 1,
                });
        }

        [HttpGet("statistic/new")]
        public async Task<IActionResult> GetOverallCost()
        {
            var users = await _userManager.GetUsersInRoleAsync("Customer");
            var usersInThisMonth = users.Where(user => user.DateCreate >= DateTime.UtcNow.AddDays(-30)).ToList();
            var usersInThisWeek = usersInThisMonth.Where(user => user.DateCreate >= DateTime.UtcNow.AddDays(-7)).ToList();
            return Ok(new
            {
                pending = users.Where(user => user.DateAsPartner == null).ToList().Count,
                thisMonth = usersInThisMonth.Count,
                thisWeek = usersInThisWeek.Count,
                total = users.Count
            });
        }

        [HttpPut("{id}/processed")]
        public async Task<IActionResult> ProccessUser([FromRoute]string id)
        {
            var customer = await _userManager.FindByIdAsync(id) ?? throw new Exception("User is not found");
            customer.DateAsPartner = DateTime.UtcNow;
            customer.PhoneNumberConfirmed = true;
            await _userManager.UpdateAsync(customer);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsers([FromRoute] string id)
        {
            var customers = await _userManager.FindByIdAsync(id) ?? throw new Exception("User is not found");
            await _userManager.DeleteAsync(customers);
            return NoContent();
        }
    }
}
