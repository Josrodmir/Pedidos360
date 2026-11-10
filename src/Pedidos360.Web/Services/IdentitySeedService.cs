namespace Pedidos360.Web.Services;

/// <summary>
/// Creates the application roles after Azure SQL becomes available without
/// delaying Kestrel startup while a serverless database is resuming.
/// </summary>
public sealed class IdentitySeedService(
    IServiceScopeFactory scopeFactory,
    ILogger<IdentitySeedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Let the web host accept requests before attempting the remote database.
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            using var scope = scopeFactory.CreateScope();
            await IdentitySeeder.SeedAsync(scope.ServiceProvider);
            logger.LogInformation("Identity roles verified: Administrador, Ventas, Contabilidad and Inventario.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception,
                "Identity roles will be verified on the next application start after Azure SQL is available.");
        }
    }
}
