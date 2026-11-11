namespace Pedidos360.Web.Services;

/// <summary>
/// Creates the application roles after the configured database becomes available
/// without delaying Kestrel startup while a remote service is resuming.
/// </summary>
public sealed class IdentitySeedService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<IdentitySeedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Let the web host accept requests before attempting the remote database.
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Pedidos360.Web.Data.ApplicationDbContext>();
            if (context.Database.ProviderName?.Contains("MySql", StringComparison.OrdinalIgnoreCase) == true)
                await context.Database.EnsureCreatedAsync(stoppingToken);
            await IdentitySeeder.SeedAsync(scope.ServiceProvider, configuration);
            logger.LogInformation("Identity roles and configured demo accounts verified.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception,
                "Identity roles will be verified on the next application start after the database is available.");
        }
    }
}
