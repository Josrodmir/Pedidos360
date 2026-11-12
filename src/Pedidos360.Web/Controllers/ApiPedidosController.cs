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
        if (lineas.Any(l => l.Cantidad <= 0 || l.Descuento is < 0 or > 100))
            return BadRequest("Revise las cantidades y descuentos ingresados.");

        var cantidadesPorProducto = lineas
            .GroupBy(l => l.ProductoId)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Cantidad));
        var productoIds = cantidadesPorProducto.Keys.ToList();
        var productos = await context.Productos.Where(p => productoIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        if (productos.Count != cantidadesPorProducto.Count) return BadRequest("Uno o más productos no existen.");

        var sinStock = cantidadesPorProducto
            .Where(item => !productos[item.Key].Activo || productos[item.Key].Stock < item.Value)
            .Select(item => productos[item.Key].Nombre)
            .Distinct()
            .ToList();
        if (sinStock.Count > 0)
            return BadRequest($"Stock insuficiente o producto inactivo: {string.Join(", ", sinStock)}.");

        var subtotal = lineas.Sum(l => productos[l.ProductoId].Precio * l.Cantidad * (1 - l.Descuento / 100));
        var impuestos = lineas.Sum(l => productos[l.ProductoId].Precio * l.Cantidad * (1 - l.Descuento / 100) * productos[l.ProductoId].ImpuestoPorc / 100);
        return Ok(new { subtotal, impuestos, total = subtotal + impuestos });
    }
}
