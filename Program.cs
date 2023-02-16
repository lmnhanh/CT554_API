using CT554_API.Data;
using CT554_API.Entity;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<CT554DbContext>(option =>{
	option.UseSqlServer(builder.Configuration.GetConnectionString("MyDatabase"));
});

builder.Services.AddIdentity<User, IdentityRole>()
	.AddEntityFrameworkStores<CT554DbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(
		policy =>
		{
			policy.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader();
		});
});

//builder.Services.AddAuthentication("Bearer")
//	.AddJwtBearer("Bearer", options =>
//	{
//		options.Authority = "https://localhost:5001";
//		options.TokenValidationParameters = new TokenValidationParameters
//		{
//			ValidateAudience = false,
//			ValidTypes = new[] { "at+jwt" }
//		};
//	});

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("Admin", builder =>
	{
		builder.RequireClaim("scope", new[] { "Admin" });
	});
	options.AddPolicy("Customer", builder =>
	{
		builder.RequireClaim("scope", new[] { "Customer" });
	});
});

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
	options.Authority = "https://localhost:5001";
	options.SaveToken = true;
	//options.RequireHttpsMetadata = false;
	options.TokenValidationParameters = new TokenValidationParameters()
	{
		ValidateIssuer = true,
		ValidateAudience = false,
		//ValidAudience = configuration["JWT:ValidAudience"],
		ValidIssuer = configuration["JWT:ValidIssuer"],
		ValidTypes = new[] { "at+jwt" }
	};
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>{
		options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
	});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
