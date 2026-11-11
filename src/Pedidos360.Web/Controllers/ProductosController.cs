using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Web.Data;
using Pedidos360.Web.Models;
using Pedidos360.Web.Security;

namespace Pedidos360.Web.Controllers;

public class ProductosController(ApplicationDbContext context) : Controller
{
    private const int PageSize = 8;
    private const long MaxImageSizeBytes = 2 * 1024 * 1024;
    private static readonly HashSet<string> AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? busqueda, int? categoriaId, int pagina = 1)
    {
        var query = context.Productos.Include(p => p.Categoria).AsNoTracking().AsQueryable();

        if (!User.IsInRole(AppRoles.Administrador) && !User.IsInRole(AppRoles.Inventario))
        {
            query = query.Where(p => p.Activo);
        }

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

    [Authorize(Roles = AppRoles.CatalogoGestion)]
    public async Task<IActionResult> Create()
    {
        await CargarCategoriasAsync();
        return View(new Producto());
    }

    [Authorize(Roles = AppRoles.CatalogoGestion)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto producto)
    {
        if (!await context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId))
        {
            ModelState.AddModelError(nameof(producto.CategoriaId), "Seleccione una categoría válida.");
        }

        ValidarImagen(producto.ImagenArchivo, requerida: true);

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(producto.CategoriaId);
            return View(producto);
        }

        await GuardarImagenAsync(producto, producto.ImagenArchivo!);
        context.Add(producto);
        await context.SaveChangesAsync();
        TempData["Success"] = "Producto creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.CatalogoGestion)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var producto = await context.Productos.FindAsync(id);
        if (producto is null) return NotFound();
        await CargarCategoriasAsync(producto.CategoriaId);
        return View(producto);
    }

    [Authorize(Roles = AppRoles.CatalogoGestion)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.Id) return NotFound();
        var productoExistente = await context.Productos.FindAsync(id);
        if (productoExistente is null) return NotFound();

        if (!await context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId))
        {
            ModelState.AddModelError(nameof(producto.CategoriaId), "Seleccione una categoría válida.");
        }

        ValidarImagen(producto.ImagenArchivo, requerida: false);

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(producto.CategoriaId);
            return View(producto);
        }

        productoExistente.Nombre = producto.Nombre;
        productoExistente.CategoriaId = producto.CategoriaId;
        productoExistente.Precio = producto.Precio;
        productoExistente.ImpuestoPorc = producto.ImpuestoPorc;
        productoExistente.Stock = producto.Stock;
        productoExistente.Activo = producto.Activo;

        if (producto.ImagenArchivo is not null)
        {
            await GuardarImagenAsync(productoExistente, producto.ImagenArchivo);
        }

        await context.SaveChangesAsync();

        TempData["Success"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var producto = await context.Productos.Include(p => p.Categoria).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return producto is null ? NotFound() : View(producto);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Imagen(int id)
    {
        var imagen = await context.Productos.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new { p.ImagenBytes, p.ImagenTipo })
            .FirstOrDefaultAsync();

        return imagen?.ImagenBytes is { Length: > 0 }
            ? File(imagen.ImagenBytes, imagen.ImagenTipo ?? "application/octet-stream")
            : NotFound();
    }

    [Authorize(Roles = AppRoles.Administrador)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var producto = await context.Productos.Include(p => p.Categoria).AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return producto is null ? NotFound() : View(producto);
    }

    [Authorize(Roles = AppRoles.Administrador)]
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

    private void ValidarImagen(IFormFile? archivo, bool requerida)
    {
        if (archivo is null || archivo.Length == 0)
        {
            if (requerida)
            {
                ModelState.AddModelError(nameof(Producto.ImagenArchivo), "Seleccione una imagen JPG, PNG o WEBP.");
            }
            return;
        }

        if (!AllowedImageTypes.Contains(archivo.ContentType))
        {
            ModelState.AddModelError(nameof(Producto.ImagenArchivo), "La imagen debe ser JPG, PNG o WEBP.");
        }

        if (archivo.Length > MaxImageSizeBytes)
        {
            ModelState.AddModelError(nameof(Producto.ImagenArchivo), "La imagen no puede superar 2 MB.");
        }
    }

    private static async Task GuardarImagenAsync(Producto producto, IFormFile archivo)
    {
        await using var stream = new MemoryStream();
        await archivo.CopyToAsync(stream);
        producto.ImagenBytes = stream.ToArray();
        producto.ImagenTipo = archivo.ContentType;
        producto.ImagenUrlLegado = null;
    }
}
