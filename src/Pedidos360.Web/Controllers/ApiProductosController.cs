using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Web.Data;

namespace Pedidos360.Web.Controllers;

[ApiController, Route("api/productos"), Authorize]
public class ApiProductosController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar(string? q) => Ok(await context.Productos.AsNoTracking()
        .Where(p => p.Activo && p.Stock > 0 && (string.IsNullOrWhiteSpace(q) || p.Nombre.Contains(q)))
        .OrderBy(p => p.Nombre).Take(10)
        .Select(p => new { id = p.Id, nombre = p.Nombre, precio = p.Precio, impuesto = p.ImpuestoPorc, stock = p.Stock })
        .ToListAsync());
}
