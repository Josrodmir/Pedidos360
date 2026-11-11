using Microsoft.AspNetCore.Identity;
using Pedidos360.Web.Security;

namespace Pedidos360.Web.Services;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roles = new[] { AppRoles.Administrador, AppRoles.Ventas, AppRoles.Contabilidad, AppRoles.Inventario };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                    throw new InvalidOperationException($"No se pudo crear el rol {role}.");
            }
        }

        // Demo identities are opt-in: credentials are provided only as deployment secrets.
        if (!configuration.GetValue<bool>("DemoAccounts:SeedEnabled")) return;

        foreach (var role in roles)
        {
            var section = configuration.GetSection($"DemoAccounts:{role}");
            var email = section["Email"];
            var password = section["Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) continue;

            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                    throw new InvalidOperationException($"No se pudo crear la cuenta demo para {role}.");
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                var roleResult = await userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException($"No se pudo asignar el rol {role}.");
            }
        }
    }
}
