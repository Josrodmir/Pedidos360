using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Web.Data;
using Pedidos360.Web.Models;

namespace Pedidos360.Web.Controllers;

public class ProductosController(ApplicationDbContext context) : Controller
{
    private const int PageSize = 8;

    public async Task<IActionResult> Index(string? busqueda, int? categoriaId, int pagina = 1)
    {
        var query = context.Productos.Include(p => p.Categoria).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(p => p.Nombre.Contains(busqueda));
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        var total = await query.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        return View(new ProductoIndexViewModel
        {
            Productos = await query.OrderBy(p => p.Nombre).Skip((pagina - 1) * PageSize).Take(PageSize).ToListAsync(),
            Categorias = await context.Categorias.OrderBy(c => c.Nombre).AsNoTracking().ToListAsync(),
            Busqueda = busqueda,
            CategoriaId = categoriaId,
            PaginaActual = pagina,
            TotalPaginas = totalPaginas
        });
    }

    public async Task<IActionResult> Create()
    {
        await CargarCategoriasAsync();
        return View(new Producto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto producto)
    {
        if (!await context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId))
        {
            ModelState.AddModelError(nameof(producto.CategoriaId), "Seleccione una categoría válida.");
        }

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(producto.CategoriaId);
            return View(producto);
        }

        context.Add(producto);
        await context.SaveChangesAsync();
        TempData["Success"] = "Producto creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var producto = await context.Productos.FindAsync(id);
        if (producto is null) return NotFound();
        await CargarCategoriasAsync(producto.CategoriaId);
        return View(producto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.Id) return NotFound();
        if (!await context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId))
        {
            ModelState.AddModelError(nameof(producto.CategoriaId), "Seleccione una categoría válida.");
        }

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(producto.CategoriaId);
            return View(producto);
        }

        try
        {
            context.Update(producto);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await context.Productos.AnyAsync(p => p.Id == producto.Id)) return NotFound();
            throw;
        }

        TempData["Success"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var producto = await context.Productos.Include(p => p.Categoria).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return producto is null ? NotFound() : View(producto);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var producto = await context.Productos.Include(p => p.Categoria).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return producto is null ? NotFound() : View(producto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var producto = await context.Productos.FindAsync(id);
        if (producto is not null)
        {
            context.Productos.Remove(producto);
            await context.SaveChangesAsync();
            TempData["Success"] = "Producto eliminado correctamente.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCategoriasAsync(object? seleccionada = null) =>
        ViewBag.CategoriaId = new SelectList(await context.Categorias.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre", seleccionada);
}
