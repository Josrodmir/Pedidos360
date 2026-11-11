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
        const int maximumAttempts = 12;

        for (var attempt = 1; attempt <= maximumAttempts && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                // Let the web host accept requests before attempting the remote database.
                await Task.Delay(TimeSpan.FromSeconds(attempt == 1 ? 2 : 5), stoppingToken);
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<Pedidos360.Web.Data.ApplicationDbContext>();
                if (context.Database.ProviderName?.Contains("MySql", StringComparison.OrdinalIgnoreCase) == true)
                    await context.Database.EnsureCreatedAsync(stoppingToken);

                await IdentitySeeder.SeedAsync(scope.ServiceProvider, configuration);
                logger.LogInformation("Identity roles and configured demo accounts verified.");
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception) when (attempt < maximumAttempts)
            {
                logger.LogWarning(exception,
                    "Identity initialization attempt {Attempt}/{MaximumAttempts} failed; retrying after the database becomes available.",
                    attempt,
                    maximumAttempts);
            }
            catch (Exception exception)
            {
                logger.LogError(exception,
                    "Identity initialization could not finish after {MaximumAttempts} attempts.",
                    maximumAttempts);
            }
        }
    }
}
