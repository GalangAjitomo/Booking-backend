using Booking.Api.Data;
using Booking.Api.Middleware;
using Booking.Api.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================== DATABASE ==================
var conn = builder.Configuration.GetConnectionString("Default")
           ?? "Server=(localdb)\\mssqllocaldb;Database=BookingDB;Trusted_Connection=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(conn));

// ================== IDENTITY ==================
builder.Services.AddIdentity<ApplicationUserModel, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ================== JWT AUTH (OFFICIAL WAY) ==================
var jwtKey = builder.Configuration["Jwt:Key"]
             ?? "ReplaceThisSecretKey_ChangeIt!";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// ================== OFFICIAL .NET OPENAPI ==================
builder.Services.AddOpenApi();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ================== MIDDLEWARE ORDER (MS RECOMMENDED) ==================
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// expose OpenAPI document
app.MapOpenApi(); // -> /openapi/v1.json
app.MapScalarApiReference(options =>
{
    options.Title = "Booking API";
    options.Theme = ScalarTheme.Default;
});

// ================== ENDPOINTS ==================
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapRoomEndpoints();    
app.MapBookingEndpoints(); 
app.UseCors("CorsPolicy");

// ================== SEED ==================
await SeedData.EnsureSeedAsync(app.Services);
app.Run();
