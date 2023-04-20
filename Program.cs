using CT554_API.Auth;
using CT554_API.Config.Middleware;
using CT554_Entity.Data;
using CT554_Entity.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddDbContext<CT554DbContext>(option =>
{
    option.UseSqlServer(
        builder.Configuration.GetConnectionString("MyDatabase"),
        options => options.MigrationsAssembly("CT554_API"));
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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
//builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidTypes = new[] { "at+jwt" }
        };
    });

builder.Services.AddAuthorization(options =>
{
    //options.AddPolicy("Admin", builder =>
    //{
    //    builder.RequireClaim("scope", new[] { "Admin" });
    //});
    //options.AddPolicy("Customer", builder =>
    //{
    //    builder.RequireClaim("scope", new[] { "Customer" });
    //});
    options.AddPolicy("Admin", builder =>
    {
        builder.AddRequirements(new RequirementRoleClaim("Admin"));
    });
});


//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(options =>
//{
//    options.SaveToken = true;
//    options.RequireHttpsMetadata = false;
//    options.TokenValidationParameters = new TokenValidationParameters()
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidAudience = configuration["JWT:ValidAudience"],
//        ValidIssuer = configuration["JWT:ValidIssuer"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret:Admin"] ?? ""))
//    };
//});

builder.Services.AddControllers();
builder.Services.AddScoped<IAuthorizationHandler, PoliciesAuthorizationHandler>();
//.AddNewtonsoftJson(options =>{
//	options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
//});

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//	app.UseSwagger();
//	app.UseSwaggerUI();
//}
//app.UseMiddleware<CustomExceptionHandlerMiddleware>();
app.UseHttpsRedirection();

app.UseCors();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
