using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Web.Data;
using Pedidos360.Web.Security;

namespace Pedidos360.Web.Controllers;

[ApiController, Route("api/productos")]
public class ApiProductosController(ApplicationDbContext context) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("filtrar")]
    public async Task<IActionResult> Filtrar(string? q, int? categoriaId)
    {
        var query = context.Productos.Include(p => p.Categoria).AsNoTracking().Where(p => p.Activo).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Nombre.Contains(q));
        if (categoriaId.HasValue) query = query.Where(p => p.CategoriaId == categoriaId.Value);

        return Ok(await query.OrderBy(p => p.Nombre).Take(100).Select(p => new
        {
            p.Id,
            p.Nombre,
            Categoria = p.Categoria!.Nombre,
            p.Precio,
            p.ImpuestoPorc,
            p.Stock,
            p.Activo
        }).ToListAsync());
    }

    [AllowAnonymous]
    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar(string? q) => Ok(await context.Productos.AsNoTracking()
        .Where(p => p.Activo && p.Stock > 0 && (string.IsNullOrWhiteSpace(q) || p.Nombre.Contains(q)))
        .OrderBy(p => p.Nombre).Take(10)
        .Select(p => new { id = p.Id, nombre = p.Nombre, precio = p.Precio, impuesto = p.ImpuestoPorc, stock = p.Stock })
        .ToListAsync());
}
