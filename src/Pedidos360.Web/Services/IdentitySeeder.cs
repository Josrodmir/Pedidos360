using Microsoft.AspNetCore.Identity;
using Pedidos360.Web.Security;

namespace Pedidos360.Web.Services;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { AppRoles.Administrador, AppRoles.Ventas, AppRoles.Contabilidad, AppRoles.Inventario })
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));
    }
}
