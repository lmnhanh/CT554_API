using CT554_API.Entity;
using CT554_API.Models.Auth;
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
			//	var userRoles = await _userManager.GetRolesAsync(user);

			//	var authClaims = new List<Claim>
			//	{
			//		new Claim("username", user.UserName ?? ""),
			//		new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			//	};

			//	foreach (var userRole in userRoles)
			//	{
			//		authClaims.Add(new Claim("role", userRole));
			//	}

			//	var token = GetToken(authClaims);

			//	return Ok(new
			//	{
			//		access_token = new JwtSecurityTokenHandler().WriteToken(token),
			//		expiration = token.ValidTo
			//	});
			//}
			//return Unauthorized();

			if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
			{
				string role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "";
				var values = new Dictionary<string, string>
				{
					{ "client_id", "Admin_LmA7@!@D" },
					{ "client_secret", _configuration[$"JWT:Secret:{role}"]?? "" },
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
			var userExists = await _userManager.FindByNameAsync(model.Username);
			if (userExists != null)
				return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

			User user = new User
			{
				FullName = model.Username,
				Email = model.Email,
				SecurityStamp = Guid.NewGuid().ToString(),
				UserName = model.Username
			};
			var result = await _userManager.CreateAsync(user, model.Password);
			if (!result.Succeeded)
				return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = result.Errors.First().Description, Message = "User creation failed! Please check user details and try again." });
            await _userManager.AddToRoleAsync(user, Roles.Customer);
            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
		}

		[HttpPost]
		[Route("register-admin")]
		public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
		{
			var userExists = await _userManager.FindByNameAsync(model.Username);
			if (userExists != null)
				return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

			User user = new()
			{
				Email = model.Email,
				SecurityStamp = Guid.NewGuid().ToString(),
				UserName = model.Username
			};
			var result = await _userManager.CreateAsync(user, model.Password);
			if (!result.Succeeded)
				return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

			if (!await _roleManager.RoleExistsAsync(Roles.Admin))
				await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));
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
			return Ok(new Response { Status = "Success", Message = "User created successfully!" });
		}

		private JwtSecurityToken GetToken(List<Claim> authClaims)
		{
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret:Admin"]?? ""));

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
