using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Web.Data;
using Pedidos360.Web.Models;
using Pedidos360.Web.Security;

namespace Pedidos360.Web.Controllers;

public class PedidosController(ApplicationDbContext context) : Controller
{
    [Authorize(Roles = AppRoles.PedidosLectura)]
    public async Task<IActionResult> Index() => View(await context.Pedidos.Include(p => p.Cliente).AsNoTracking().OrderByDescending(p => p.Fecha).ToListAsync());
    [Authorize(Roles = AppRoles.PedidosGestion)]
    public async Task<IActionResult> Create() { await Clientes(); return View(); }
    [Authorize(Roles = AppRoles.PedidosGestion)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PedidoEntrada entrada)
    {
        if (entrada.Lineas is null || entrada.Lineas.Count == 0) ModelState.AddModelError("", "Agregue al menos un producto.");
        if (!await context.Clientes.AnyAsync(c => c.Id == entrada.ClienteId)) ModelState.AddModelError("", "Seleccione un cliente válido.");
        var ids = entrada.Lineas?.Select(x => x.ProductoId).Distinct().ToList() ?? [];
        var productos = await context.Productos.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        foreach (var l in entrada.Lineas ?? []) if (!productos.TryGetValue(l.ProductoId, out var p) || !p.Activo || p.Stock < l.Cantidad) ModelState.AddModelError("", "Un producto no está disponible o no tiene stock suficiente.");
        if (!ModelState.IsValid) { await Clientes(); return View(entrada); }
        var pedido = new Pedido { ClienteId = entrada.ClienteId, UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) };
        foreach (var l in entrada.Lineas!) { var p = productos[l.ProductoId]; var baseLinea = p.Precio * l.Cantidad * (1 - l.Descuento / 100); p.Stock -= l.Cantidad; pedido.Detalles.Add(new PedidoDetalle { ProductoId=p.Id, Cantidad=l.Cantidad, PrecioUnitario=p.Precio, Descuento=l.Descuento, ImpuestoPorc=p.ImpuestoPorc, TotalLinea=baseLinea + baseLinea*p.ImpuestoPorc/100 }); }
        pedido.Subtotal = pedido.Detalles.Sum(d => d.PrecioUnitario*d.Cantidad*(1-d.Descuento/100)); pedido.Impuestos = pedido.Detalles.Sum(d => d.PrecioUnitario*d.Cantidad*(1-d.Descuento/100)*d.ImpuestoPorc/100); pedido.Total=pedido.Subtotal+pedido.Impuestos;
        context.Pedidos.Add(pedido); await context.SaveChangesAsync(); TempData["Success"]="Pedido confirmado y stock actualizado."; return RedirectToAction(nameof(Details), new { id=pedido.Id });
    }
    [Authorize(Roles = AppRoles.PedidosLectura)]
    public async Task<IActionResult> Details(int id) { var p=await context.Pedidos.Include(x=>x.Cliente).Include(x=>x.Detalles).ThenInclude(x=>x.Producto).AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id); return p is null ? NotFound() : View(p); }
    private async Task Clientes() => ViewBag.ClienteId=new SelectList(await context.Clientes.OrderBy(c=>c.Nombre).ToListAsync(),"Id","Nombre");
}
