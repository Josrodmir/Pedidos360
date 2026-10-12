using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Web.Data;
using Pedidos360.Web.Models;

namespace Pedidos360.Web.Controllers;

public class ClientesController(ApplicationDbContext context) : Controller
{
    private const int PageSize = 8;

    public async Task<IActionResult> Index(string? busqueda, int pagina = 1)
    {
        var query = context.Clientes.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(c => c.Nombre.Contains(busqueda) || c.Cedula.Contains(busqueda));
        }

        var total = await query.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        return View(new ClienteIndexViewModel
        {
            Clientes = await query.OrderBy(c => c.Nombre).Skip((pagina - 1) * PageSize).Take(PageSize).ToListAsync(),
            Busqueda = busqueda,
            PaginaActual = pagina,
            TotalPaginas = totalPaginas
        });
    }

    public IActionResult Create() => View(new Cliente());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cliente cliente)
    {
        if (await context.Clientes.AnyAsync(c => c.Cedula == cliente.Cedula))
            ModelState.AddModelError(nameof(cliente.Cedula), "Ya existe un cliente con esta identificación.");
        if (!ModelState.IsValid) return View(cliente);
        context.Add(cliente);
        await context.SaveChangesAsync();
        TempData["Success"] = "Cliente creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var cliente = await context.Clientes.FindAsync(id);
        return cliente is null ? NotFound() : View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Cliente cliente)
    {
        if (id != cliente.Id) return NotFound();
        if (await context.Clientes.AnyAsync(c => c.Cedula == cliente.Cedula && c.Id != cliente.Id))
            ModelState.AddModelError(nameof(cliente.Cedula), "Ya existe un cliente con esta identificación.");
        if (!ModelState.IsValid) return View(cliente);
        try
        {
            context.Update(cliente);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await context.Clientes.AnyAsync(c => c.Id == cliente.Id)) return NotFound();
            throw;
        }
        TempData["Success"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var cliente = await context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return cliente is null ? NotFound() : View(cliente);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var cliente = await context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return cliente is null ? NotFound() : View(cliente);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await context.Clientes.FindAsync(id);
        if (cliente is not null)
        {
            context.Clientes.Remove(cliente);
            await context.SaveChangesAsync();
            TempData["Success"] = "Cliente eliminado correctamente.";
        }
        return RedirectToAction(nameof(Index));
    }
}
