
using CT554_API.Models.Auth;
using CT554_Entity.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CT554_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticateController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("/discovery")]
        [Authorize]
        public IActionResult Discovery()
        {
            return Ok();
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            //if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            //{
            //    var userRoles = await _userManager.GetRolesAsync(user);

            //    var authClaims = new List<Claim>
            //    {
            //        new Claim(ClaimTypes.Name, user.UserName ?? ""),
            //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //    };

            //    foreach (var userRole in userRoles)
            //    {
            //        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            //    }

            //    var token = GetToken(authClaims);

            //    List<string> clientScopes = new();
            //    foreach (var roleName in await _userManager.GetRolesAsync(user))
            //    {
            //        var clientRole = await _roleManager.FindByNameAsync(roleName);
            //        if (clientRole is not null)
            //        {
            //            clientScopes.AddRange(
            //                (await _roleManager.GetClaimsAsync(clientRole))
            //                .Select(claim => claim.Value)
            //                .ToList()
            //            );
            //        }
            //    }

            //    return Ok(new
            //    {
            //        access_token = new JwtSecurityTokenHandler().WriteToken(token),
            //        expiration = token.ValidTo,
            //        clientId = user.Id,
            //        secret = user.PasswordHash,
            //        role = await _userManager.GetRolesAsync(user),
            //        scopes = clientScopes
            //    });
            //}
            //return Unauthorized();

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                string role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "";
                var values = new Dictionary<string, string>
                {
                    { "client_id", user.Id },
                    { "client_secret", user.PasswordHash ?? ""},
                    { "scope", role },
                    {"grant_type", "client_credentials" }
                };

                var data = new FormUrlEncodedContent(values);

                var url = "https://localhost:5001/connect/token";
                using var client = new HttpClient();
                var response = await client.PostAsync(url, data);

                string result = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<ConnectModel>(result);

                return Ok(responseObject);
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Email);
            if(_userManager.Users.Any(user => user.PhoneNumber== model.PhoneNumber && user.PhoneNumberConfirmed))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { phoneNumber = "Số điện thoại đã tồn tại" });
            }
            if (userExists != null)
                return StatusCode(StatusCodes.Status400BadRequest, new { email = "Email đã tồn tại" });

            User user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber= model.PhoneNumber,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            await _userManager.AddToRoleAsync(user, Roles.Customer);
            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Email);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            User user = new()
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true,
                PhoneNumberConfirmed= true,
                UserName = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            if (!await _roleManager.RoleExistsAsync(Roles.Admin)) {
               await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));
               var createdRole = await _roleManager.FindByNameAsync(Roles.Admin);
               await _roleManager.AddClaimAsync(createdRole!, new Claim("Permission", "ProductAdd"));
            }
                if (!await _roleManager.RoleExistsAsync(Roles.Customer))
                await _roleManager.CreateAsync(new IdentityRole(Roles.Customer));

            if (await _roleManager.RoleExistsAsync(Roles.Admin))
            {
                await _userManager.AddToRoleAsync(user, Roles.Admin);
            }
            //if (await _roleManager.RoleExistsAsync(Roles.Admin))
            //{
            //	await _userManager.AddToRoleAsync(user, Roles.Customer);
            //}
            return Ok(new
            {
                Status = "Success",
                Message = "User created successfully!",
                ClientId = user.Id,
                Secret = user.PasswordHash,
                Scope = await _userManager.GetRolesAsync(user)
            }); ;
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret:Admin"] ?? ""));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}
