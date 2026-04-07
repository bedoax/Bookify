using Bookify.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    /// <summary>
    /// Background service that runs nightly to purge expired and revoked
    /// refresh tokens, keeping the RefreshTokens table from growing unbounded.
    /// </summary>
    public class RefreshTokenCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RefreshTokenCleanupWorker> _logger;

        // Tokens older than this threshold are deleted even if not explicitly revoked.
        private static readonly TimeSpan TokenRetentionPeriod = TimeSpan.FromDays(30);

        // Run once every 24 hours.
        private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);

        public RefreshTokenCleanupWorker(
            IServiceProvider serviceProvider,
            ILogger<RefreshTokenCleanupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RefreshToken Cleanup Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredTokensAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during refresh token cleanup.");
                }

                // Wait 24 hours before the next run.
                await Task.Delay(RunInterval, stoppingToken);
            }

            _logger.LogInformation("RefreshToken Cleanup Worker stopped.");
        }

        private async Task CleanupExpiredTokensAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cutoffDate = DateTime.UtcNow - TokenRetentionPeriod;

            // Delete tokens that are either:
            //   (a) explicitly revoked, OR
            //   (b) past their expiry date by more than the retention period.
            var deleted = await context.RefreshTokens
                .Where(t => t.Revoked || t.ExpiresAt < cutoffDate)
                .ExecuteDeleteAsync(ct);

            if (deleted > 0)
            {
                _logger.LogInformation(
                    "RefreshToken cleanup: deleted {Count} expired/revoked tokens (cutoff: {Cutoff:u}).",
                    deleted, cutoffDate);
            }
        }
    }
}
