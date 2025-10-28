using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceSimulacro.Data;
using EcommerceSimulacro.Models;

namespace EcommerceSimulacro.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly AppDbContext _db; public CompaniesController(AppDbContext db)=>_db=db;

    [HttpGet]
    public IActionResult Get() => Ok(_db.Companies.ToList());

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public IActionResult Create(Company c){
        _db.Companies.Add(c); _db.SaveChanges();
        return Created($"/api/companies/{c.Id}", c);
    }
}