using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sweet_Silvia_Domain.RefreshToken;

namespace Sweet_Silvia_Infrastructure;

internal sealed class RefreshTokenCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<RefreshTokenCleanupService> logger)
    : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CleanupAsync(stoppingToken);

        using var timer = new PeriodicTimer(CleanupInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CleanupAsync(stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var refreshTokens = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
            var deleted = await refreshTokens.DeleteInactiveAsync(DateTimeOffset.UtcNow, cancellationToken);

            if (deleted > 0)
            {
                logger.LogInformation("Removed {Count} inactive refresh tokens.", deleted);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The host is stopping.
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not clean inactive refresh tokens. It will retry on the next interval.");
        }
    }
}
