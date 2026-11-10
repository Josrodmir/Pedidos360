using Microsoft.EntityFrameworkCore;
using Pedidos360.Web.Data;

namespace Pedidos360.Web.Services;

/// <summary>
/// Requests Azure SQL to resume as soon as the web application starts.
/// Azure SQL Serverless can pause after inactivity; this lightweight query
/// avoids making the first business page responsible for waking the database.
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
                await context.Database.ExecuteSqlRawAsync("SELECT 1", stoppingToken);
            });

            logger.LogInformation("Azure SQL is available after application warm-up.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The application is stopping; no action is required.
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception,
                "Azure SQL is still resuming. Request retries will continue using the configured strategy.");
        }
    }
}
