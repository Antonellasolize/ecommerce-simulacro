using System.Text;
using EcommerceSimulacro.Data;
using EcommerceSimulacro.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// DB
var cs = builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(cs))
{
    cs = "Data Source=app.db"; // SQLite local fallback
    builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(cs));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(cs));
}

// JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "DEV_KEY_cambiar_en_produccion";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => {
        o.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key
        };
    });

builder.Services.AddAuthorization(options => {
    options.AddPolicy("Admin",   p => p.RequireClaim("role", Role.Admin.ToString()));
    options.AddPolicy("Empresa", p => p.RequireClaim("role", Role.Empresa.ToString()));
    options.AddPolicy("Cliente", p => p.RequireClaim("role", Role.Cliente.ToString()));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

var app = builder.Build();

// Migrar y sembrar con protección
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("ERROR MIGRANDO BD >>> " + ex.Message);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();