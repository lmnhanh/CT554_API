
using CT554_API.Models.Auth;
using CT554_Entity.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace CT554_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public AuthenticateController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender ,IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailSender = emailSender;
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
			if (user == null)
			{
				return BadRequest("Thông tin đăng nhập không chính xác");
			}

			if (!user!.EmailConfirmed)
            {
                return BadRequest("Email chưa được xác minh");
            }

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
                var responseObject = JsonConvert.DeserializeObject<ConnectModel>(result) ?? throw new Exception("Fail to authenticate");
                responseObject.IsPasswordDefault = model.Password == "X2Uqxe&3k@";
                responseObject.IsConfirmEmail = user.EmailConfirmed;
                responseObject.Username = user.UserName ?? "";
                return Ok(responseObject);
            }
            return BadRequest("Thông tin đăng nhập không chính xác");
        }

		[HttpPost("confirmEmail/{id}")]
        public async Task<IActionResult> ConfirmEmail([FromRoute] string id, [FromBody] EmailToken model)
        {
            var user = await _userManager.FindByIdAsync(id) ?? throw new Exception("User is not found");
            var result = await _userManager.ConfirmEmailAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token)));
            if (!result.Succeeded)
            {
                return BadRequest();
            }
            return Ok();
        }

		[HttpPost("sendConfirmEmail/{id}")]
		public async Task<IActionResult> SetConfirmEmail([FromRoute] string id)
		{
			var user = await _userManager.FindByIdAsync(id) ?? throw new Exception("User is not found");
			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
			await _emailSender.SendEmailAsync(user.Email!, "Xác nhận email của bạn",
			$"Cảm ơn bạn đã đăng kí với cửa hàng. Vui lòng nhấn <a href='{HtmlEncoder.Default.Encode($"http://localhost:3000/shop/login?user={user.Id}&token={token}")}'>vào đây</a> để xác minh email và mua hàng thôi !!");
			return Ok();
		}

		[HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Email);
            if (_userManager.Users.Any(user => user.PhoneNumber == model.PhoneNumber && user.PhoneNumberConfirmed))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { phoneNumber = "Số điện thoại đã tồn tại" });
            }
            if (userExists != null)
                return StatusCode(StatusCodes.Status400BadRequest, new { email = "Email is existed" });

            User user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                EmailConfirmed = false,
                PhoneNumber = model.PhoneNumber,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            await _userManager.AddToRoleAsync(user, Roles.Customer);
			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
			await _emailSender.SendEmailAsync(user.Email!, "Xác nhận email của bạn",
			$"Cảm ơn bạn đã đăng kí với cửa hàng. Vui lòng nhấn <a href='http://localhost:3000/shop/login?user={user.Id}&token={token}'>vào đây</a> để xác minh email và mua hàng thôi !!");
			return Ok(new Response { Status = "Success", Message = user.Id });
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
                PhoneNumberConfirmed = true,
                UserName = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            if (!await _roleManager.RoleExistsAsync(Roles.Admin))
            {
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
