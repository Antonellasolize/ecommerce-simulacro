using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EcommerceSimulacro.Data;
using EcommerceSimulacro.DTOs;
using EcommerceSimulacro.Models;

namespace EcommerceSimulacro.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db; private readonly IConfiguration _cfg;
    public AuthController(AppDbContext db, IConfiguration cfg){_db=db; _cfg=cfg;}

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest r)
    {
        var u = await _db.Users.Include(x=>x.Company).FirstOrDefaultAsync(x => x.Email == r.Email);
        if (u == null || u.Password != r.Password) return Unauthorized(); // sin BCrypt
        var token = CreateJwt(u);
        return new LoginResponse(token, u.Role.ToString(), u.CompanyId);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest r)
    {
        if (await _db.Users.AnyAsync(x => x.Email == r.Email)) return BadRequest("Email en uso");
        if (!Enum.TryParse<Role>(r.Role, true, out var role)) return BadRequest("Rol inválido");
        var user = new User { Email = r.Email, Role = role, CompanyId = r.CompanyId, Password = r.Password };
        _db.Users.Add(user); await _db.SaveChangesAsync();
        return Ok();
    }

    private string CreateJwt(User u)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new []{
            new Claim(JwtRegisteredClaimNames.Sub, u.Email),
            new Claim("uid", u.Id.ToString()),
            new Claim("role", u.Role.ToString()),
            new Claim("companyId", u.CompanyId?.ToString() ?? "")
        };
        var jwt = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}