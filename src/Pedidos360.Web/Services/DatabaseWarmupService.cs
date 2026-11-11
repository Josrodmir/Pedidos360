using Microsoft.EntityFrameworkCore;
using Pedidos360.Web.Data;

namespace Pedidos360.Web.Services;

/// <summary>
/// Initializes a newly configured relational database and verifies connectivity
/// without making the first business page responsible for this work.
/// </summary>
public sealed class DatabaseWarmupService(
    IServiceScopeFactory scopeFactory,
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
}
