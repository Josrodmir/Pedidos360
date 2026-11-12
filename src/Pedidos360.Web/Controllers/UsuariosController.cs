using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Web.Models;
using Pedidos360.Web.Security;

namespace Pedidos360.Web.Controllers;

[Authorize(Roles = AppRoles.Administrador)]
public class UsuariosController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
{
    public async Task<IActionResult> Index(string? busqueda, string? rol)
    {
        var query = userManager.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim();
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(termino)) ||
                (u.UserName != null && u.UserName.Contains(termino)));
        }

        var usuarios = await query.OrderBy(u => u.Email).ToListAsync();
        var modelo = new List<UsuarioRolViewModel>();
        foreach (var usuario in usuarios)
        {
            var rolActual = (await userManager.GetRolesAsync(usuario)).SingleOrDefault() ?? "Sin asignar";
            modelo.Add(new UsuarioRolViewModel { UsuarioId = usuario.Id, Correo = usuario.Email ?? usuario.UserName ?? "Sin correo", RolActual = rolActual });
        }
        if (!string.IsNullOrWhiteSpace(rol))
            modelo = modelo.Where(u => u.RolActual == rol).ToList();

        ViewBag.Roles = AppRoles.Lista;
        ViewBag.Busqueda = busqueda;
        ViewBag.RolSeleccionado = rol;
        return View(modelo);
    }

    public IActionResult Create()
    {
        ViewBag.Roles = AppRoles.Lista;
        return View(new CrearUsuarioViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearUsuarioViewModel modelo)
    {
        if (!AppRoles.Lista.Contains(modelo.Rol) || !await roleManager.RoleExistsAsync(modelo.Rol))
            ModelState.AddModelError(nameof(modelo.Rol), "El rol seleccionado no es válido.");

        if (await userManager.FindByEmailAsync(modelo.Correo) is not null)
            ModelState.AddModelError(nameof(modelo.Correo), "Ya existe un usuario con ese correo.");

        if (!ModelState.IsValid)
        {
            ViewBag.Roles = AppRoles.Lista;
            return View(modelo);
        }

        var usuario = new IdentityUser
        {
            UserName = modelo.Correo,
            Email = modelo.Correo,
            EmailConfirmed = true
        };
        var crear = await userManager.CreateAsync(usuario, modelo.Contrasena);
        if (!crear.Succeeded)
        {
            foreach (var error in crear.Errors) ModelState.AddModelError(string.Empty, error.Description);
            ViewBag.Roles = AppRoles.Lista;
            return View(modelo);
        }

        var asignarRol = await userManager.AddToRoleAsync(usuario, modelo.Rol);
        if (!asignarRol.Succeeded)
        {
            await userManager.DeleteAsync(usuario);
            foreach (var error in asignarRol.Errors) ModelState.AddModelError(string.Empty, error.Description);
            ViewBag.Roles = AppRoles.Lista;
            return View(modelo);
        }

        TempData["Success"] = $"Usuario creado y asignado al rol {modelo.Rol}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarRol(string usuarioId, string rol)
    {
        var usuario = await userManager.FindByIdAsync(usuarioId);
        if (usuario is null) return NotFound();
        if (!AppRoles.Lista.Contains(rol) || !await roleManager.RoleExistsAsync(rol))
        {
            TempData["Error"] = "El rol seleccionado no es válido.";
            return RedirectToAction(nameof(Index));
        }

        var rolesActuales = await userManager.GetRolesAsync(usuario);
        if (rolesActuales.Contains(AppRoles.Administrador) && rol != AppRoles.Administrador &&
            (await userManager.GetUsersInRoleAsync(AppRoles.Administrador)).Count <= 1)
        {
            TempData["Error"] = "Debe permanecer al menos un Administrador activo.";
            return RedirectToAction(nameof(Index));
        }

        var quitar = await userManager.RemoveFromRolesAsync(usuario, rolesActuales);
        var agregar = quitar.Succeeded
            ? await userManager.AddToRoleAsync(usuario, rol)
            : IdentityResult.Failed(new IdentityError { Description = "No fue posible retirar el rol anterior." });
        TempData[agregar.Succeeded ? "Success" : "Error"] = agregar.Succeeded
            ? $"Rol actualizado para {usuario.Email}."
            : "No fue posible actualizar el rol.";
        return RedirectToAction(nameof(Index));
    }
}
