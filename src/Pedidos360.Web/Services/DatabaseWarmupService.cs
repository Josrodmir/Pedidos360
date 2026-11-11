using Microsoft.EntityFrameworkCore;
using Pedidos360.Web.Data;
using Pedidos360.Web.Models;

namespace Pedidos360.Web.Services;

/// <summary>
/// Initializes a newly configured relational database and verifies connectivity
/// without making the first business page responsible for this work.
/// </summary>
public sealed class DatabaseWarmupService(
    IServiceScopeFactory scopeFactory,
    IWebHostEnvironment environment,
    ILogger<DatabaseWarmupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                // Aiven uses a clean MySQL database. EnsureCreated avoids applying
                // SQL Server-specific historical migrations to that provider.
                if (context.Database.ProviderName?.Contains("MySql", StringComparison.OrdinalIgnoreCase) == true)
                    await context.Database.EnsureCreatedAsync(stoppingToken);
                await context.Database.ExecuteSqlRawAsync("SELECT 1", stoppingToken);
                await SeedCatalogoAsync(context, stoppingToken);
                await SeedPedidoDemostracionAsync(context, stoppingToken);
            });

            logger.LogInformation("Database is available after application warm-up.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The application is stopping; no action is required.
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception,
                "The database is not ready yet. Request retries will continue using the configured strategy.");
        }
    }

    private async Task SeedCatalogoAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var categorias = new[] { "Tecnología", "Oficina", "Limpieza" };
        foreach (var nombre in categorias)
        {
            if (!await context.Categorias.AnyAsync(c => c.Nombre == nombre, cancellationToken))
                context.Categorias.Add(new Categoria { Nombre = nombre });
        }

        await context.SaveChangesAsync(cancellationToken);

        var categoriasPorNombre = await context.Categorias
            .ToDictionaryAsync(c => c.Nombre, c => c.Id, cancellationToken);

        var productos = new[]
        {
            new ProductoDemo("Teclado inalámbrico", "Tecnología", 18500m, 13m, 24, "teclado-inalambrico.png"),
            new ProductoDemo("Resma papel carta", "Oficina", 3200m, 13m, 80, "resma-papel.png"),
            new ProductoDemo("Desinfectante 1L", "Limpieza", 2450m, 13m, 35, "desinfectante.png")
        };

        foreach (var demo in productos)
        {
            var rutaImagen = Path.Combine(environment.WebRootPath, "images", "catalogo", demo.ArchivoImagen);
            if (!File.Exists(rutaImagen))
            {
                logger.LogWarning("The catalog image {ImagePath} was not found; product {ProductName} was skipped.", rutaImagen, demo.Nombre);
                continue;
            }

            var productoExistente = await context.Productos
                .FirstOrDefaultAsync(p => p.Nombre == demo.Nombre, cancellationToken);

            if (productoExistente is not null)
            {
                // Preserves stock and price already changed during a demonstration,
                // while repairing legacy data that did not yet contain a binary image.
                if (productoExistente.ImagenBytes is null or { Length: 0 })
                {
                    productoExistente.ImagenBytes = await File.ReadAllBytesAsync(rutaImagen, cancellationToken);
                    productoExistente.ImagenTipo = "image/png";
                    productoExistente.ImagenUrlLegado = null;
                }
                continue;
            }

            context.Productos.Add(new Producto
            {
                Nombre = demo.Nombre,
                CategoriaId = categoriasPorNombre[demo.Categoria],
                Precio = demo.Precio,
                ImpuestoPorc = demo.ImpuestoPorc,
                Stock = demo.Stock,
                Activo = true,
                ImagenBytes = await File.ReadAllBytesAsync(rutaImagen, cancellationToken),
                ImagenTipo = "image/png"
            });
        }

        var clientes = new[]
        {
            new ClienteDemo("Distribuidora Central S.A.", "3-101-456789", "ventas@central.cr", "2222-1000", "San José"),
            new ClienteDemo("Comercial La Bodega", "3-101-987654", "compras@labodega.cr", "2233-2000", "Heredia")
        };

        foreach (var demo in clientes)
        {
            if (await context.Clientes.AnyAsync(c => c.Cedula == demo.Cedula, cancellationToken))
                continue;

            context.Clientes.Add(new Cliente
            {
                Nombre = demo.Nombre,
                Cedula = demo.Cedula,
                Correo = demo.Correo,
                Telefono = demo.Telefono,
                Direccion = demo.Direccion
            });
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Initial catalog verified and seeded with local images stored as database bytes.");
        }
    }

    private sealed record ProductoDemo(
        string Nombre,
        string Categoria,
        decimal Precio,
        decimal ImpuestoPorc,
        int Stock,
        string ArchivoImagen);

    private sealed record ClienteDemo(
        string Nombre,
        string Cedula,
        string Correo,
        string Telefono,
        string Direccion);

    private static async Task SeedPedidoDemostracionAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        // A single confirmed order lets the team demonstrate details, totals and stock reduction.
        // It is deliberately idempotent: business orders created during the demo are never replaced.
        if (await context.Pedidos.AnyAsync(cancellationToken))
            return;

        var cliente = await context.Clientes
            .FirstOrDefaultAsync(c => c.Cedula == "3-101-456789", cancellationToken);
        var productos = await context.Productos
            .Where(p => p.Nombre == "Teclado inalámbrico" || p.Nombre == "Resma papel carta")
            .ToDictionaryAsync(p => p.Nombre, cancellationToken);

        if (cliente is null
            || !productos.TryGetValue("Teclado inalámbrico", out var teclado)
            || !productos.TryGetValue("Resma papel carta", out var resma)
            || teclado.Stock < 2
            || resma.Stock < 10)
            return;

        var pedido = new Pedido
        {
            ClienteId = cliente.Id,
            UsuarioId = await context.Users
                .Where(u => u.Email == "admin@pedidos360.demo")
                .Select(u => u.Id)
                .FirstOrDefaultAsync(cancellationToken),
            Fecha = DateTime.UtcNow,
            Estado = "Confirmado"
        };

        AgregarLinea(pedido, teclado, 2);
        AgregarLinea(pedido, resma, 10);
        pedido.Subtotal = pedido.Detalles.Sum(d => d.PrecioUnitario * d.Cantidad * (1 - d.Descuento / 100));
        pedido.Impuestos = pedido.Detalles.Sum(d => d.PrecioUnitario * d.Cantidad * (1 - d.Descuento / 100) * d.ImpuestoPorc / 100);
        pedido.Total = pedido.Subtotal + pedido.Impuestos;

        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static void AgregarLinea(Pedido pedido, Producto producto, int cantidad)
    {
        var baseLinea = producto.Precio * cantidad;
        producto.Stock -= cantidad;
        pedido.Detalles.Add(new PedidoDetalle
        {
            ProductoId = producto.Id,
            Cantidad = cantidad,
            PrecioUnitario = producto.Precio,
            Descuento = 0,
            ImpuestoPorc = producto.ImpuestoPorc,
            TotalLinea = baseLinea + baseLinea * producto.ImpuestoPorc / 100
        });
    }
}
