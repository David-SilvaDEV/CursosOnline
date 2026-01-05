using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Infrastructure
using CursosOnline.Infrastructure.Persistence.Context;
using CursosOnline.Infrastructure.Persistence.Repositories;

// Domain (interfaces)
using CursosOnline.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// =======================
// 🔹 Services
// =======================

// API Controllers
builder.Services.AddControllers();

// CORS para permitir el frontend (Vite por defecto en http://localhost:5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        // Para desarrollo, permitir cualquier origen; en producción, restringir.
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Relajamos un poco las reglas para que coincida con la contraseña de ejemplo
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// Repositories
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();

// =======================
// 🔹 App
// =======================

var app = builder.Build();

// Seed de usuario por defecto
await CursosOnline.Api.IdentityDataSeeder.SeedAsync(app.Services);

// =======================
// 🔹 Middleware
// =======================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Desactivado en desarrollo para evitar redirecciones al usar HTTP

app.UseCors("FrontendPolicy");

// ⚠️ Siempre en este orden
app.UseAuthentication();
app.UseAuthorization();

// Map API Controllers
app.MapControllers();

// Redirect root URL to Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.Run();
