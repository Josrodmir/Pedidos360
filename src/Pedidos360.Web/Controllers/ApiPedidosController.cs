using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Web.Data;
using Pedidos360.Web.Models;
using Pedidos360.Web.Security;

namespace Pedidos360.Web.Controllers;

[ApiController, Route("api/pedidos"), Authorize(Roles = AppRoles.PedidosGestion)]
public class ApiPedidosController(ApplicationDbContext context) : ControllerBase
{
    [HttpPost("calcular")]
    public async Task<IActionResult> Calcular([FromBody] List<LineaPedidoEntrada>? lineas)
    {
        if (lineas is null || lineas.Count == 0) return BadRequest("Agregue al menos un producto.");
        var productos = await context.Productos.Where(p => lineas.Select(l => l.ProductoId).Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        if (productos.Count != lineas.Select(l => l.ProductoId).Distinct().Count()) return BadRequest("Uno o más productos no existen.");
        var subtotal = lineas.Sum(l => productos[l.ProductoId].Precio * l.Cantidad * (1 - l.Descuento / 100));
        var impuestos = lineas.Sum(l => productos[l.ProductoId].Precio * l.Cantidad * (1 - l.Descuento / 100) * productos[l.ProductoId].ImpuestoPorc / 100);
        return Ok(new { subtotal, impuestos, total = subtotal + impuestos });
    }
}
