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
    public async Task<IActionResult> Index()
    {
        var usuarios = await userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var modelo = new List<UsuarioRolViewModel>();
        foreach (var usuario in usuarios)
        {
            var rol = (await userManager.GetRolesAsync(usuario)).SingleOrDefault() ?? "Sin asignar";
            modelo.Add(new UsuarioRolViewModel { UsuarioId = usuario.Id, Correo = usuario.Email ?? usuario.UserName ?? "Sin correo", RolActual = rol });
        }
        ViewBag.Roles = AppRoles.Lista;
        return View(modelo);
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
