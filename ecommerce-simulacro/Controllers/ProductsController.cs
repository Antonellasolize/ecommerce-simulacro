using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceSimulacro.Data;
using EcommerceSimulacro.Models;

namespace EcommerceSimulacro.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db; public ProductsController(AppDbContext db)=>_db=db;

    [HttpGet]
    public IActionResult List([FromQuery]int? companyId) {
        var q = _db.Products.AsQueryable();
        if (companyId.HasValue) q = q.Where(p=>p.CompanyId==companyId);
        return Ok(q.ToList());
    }

    [HttpPost]
    [Authorize(Policy = "Empresa")]
    public IActionResult Create(Product p)
    {
        int userCompany = int.TryParse(User.FindFirst("companyId")?.Value, out var cid) ? cid : 0;
        if (userCompany==0) return Forbid();
        p.CompanyId = userCompany;
        _db.Products.Add(p); _db.SaveChanges();
        return Created($"/api/products/{p.Id}", p);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Empresa")]
    public IActionResult Update(int id, Product dto)
    {
        int userCompany = int.TryParse(User.FindFirst("companyId")?.Value, out var cid) ? cid : 0;
        var p = _db.Products.FirstOrDefault(x=>x.Id==id && x.CompanyId==userCompany);
        if (p==null) return NotFound();
        p.Name=dto.Name; p.Description=dto.Description; p.Price=dto.Price; p.Stock=dto.Stock;
        _db.SaveChanges(); return Ok(p);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Empresa")]
    public IActionResult Delete(int id)
    {
        int userCompany = int.TryParse(User.FindFirst("companyId")?.Value, out var cid) ? cid : 0;
        var p = _db.Products.FirstOrDefault(x=>x.Id==id && x.CompanyId==userCompany);
        if (p==null) return NotFound();
        _db.Remove(p); _db.SaveChanges(); return NoContent();
    }
}