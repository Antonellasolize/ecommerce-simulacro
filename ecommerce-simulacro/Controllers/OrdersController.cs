using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceSimulacro.Data;
using EcommerceSimulacro.DTOs;

namespace EcommerceSimulacro.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db; public OrdersController(AppDbContext db)=>_db=db;

    [HttpPost]
    [Authorize(Policy = "Cliente")]
    public IActionResult Create(CreateOrderRequest req)
    {
        int userId = int.TryParse(User.FindFirst("uid")?.Value, out var id) ? id : 0;
        if (userId==0) return Forbid();
        var order = new EcommerceSimulacro.Models.Order { UserId = userId };
        foreach (var i in req.Items)
        {
            var p = _db.Products.Find(i.ProductId) ?? throw new Exception("Producto no encontrado");
            if (p.Stock < i.Quantity) return BadRequest($"Sin stock de {p.Name}");
            p.Stock -= i.Quantity;
            order.Items.Add(new EcommerceSimulacro.Models.OrderItem {
                ProductId = p.Id, Quantity = i.Quantity, UnitPrice = p.Price
            });
        }
        _db.Orders.Add(order); _db.SaveChanges();
        return Created($"/api/orders/{order.Id}", order);
    }

    [HttpGet("mine")]
    [Authorize(Policy = "Cliente")]
    public IActionResult Mine()
    {
        int userId = int.TryParse(User.FindFirst("uid")?.Value, out var id) ? id : 0;
        var orders = _db.Orders.Where(o=>o.UserId==userId).Select(o=> new{
            o.Id, o.CreatedAt, Total = o.Items.Sum(i=>i.UnitPrice*i.Quantity),
            Items = o.Items.Select(i=> new { i.ProductId, i.Quantity, i.UnitPrice })
        }).ToList();
        return Ok(orders);
    }
}