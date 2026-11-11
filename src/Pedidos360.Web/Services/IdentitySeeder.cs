using Microsoft.AspNetCore.Identity;
using Pedidos360.Web.Security;

namespace Pedidos360.Web.Services;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<IdentityUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        foreach (var role in new[] { AppRoles.Administrador, AppRoles.Ventas, AppRoles.Contabilidad, AppRoles.Inventario })
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));

        // Demo users are deliberately opt-in. Their shared password is supplied through
        // Render/User Secrets and is never committed to source control.
        var demoPassword = configuration["DemoAccounts:Password"];
        if (string.IsNullOrWhiteSpace(demoPassword)) return;

        var demoAccounts = new[]
        {
            (Email: "admin@pedidos360.demo", Role: AppRoles.Administrador),
            (Email: "ventas@pedidos360.demo", Role: AppRoles.Ventas),
            (Email: "inventario@pedidos360.demo", Role: AppRoles.Inventario),
            (Email: "contabilidad@pedidos360.demo", Role: AppRoles.Contabilidad)
        };

        foreach (var account in demoAccounts)
        {
            var user = await users.FindByEmailAsync(account.Email);
            if (user is null)
            {
                user = new IdentityUser
                {
                    UserName = account.Email,
                    Email = account.Email,
                    EmailConfirmed = true
                };

                var createResult = await users.CreateAsync(user, demoPassword);
                if (!createResult.Succeeded)
                    throw new InvalidOperationException($"Unable to create demo user {account.Email}: " + string.Join("; ", createResult.Errors.Select(error => error.Description)));
            }

            if (!await users.IsInRoleAsync(user, account.Role))
            {
                var roleResult = await users.AddToRoleAsync(user, account.Role);
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException($"Unable to assign role {account.Role} to {account.Email}: " + string.Join("; ", roleResult.Errors.Select(error => error.Description)));
            }
        }
    }
}
