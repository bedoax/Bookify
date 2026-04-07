using Bookify.Data;
using Bookify.Models.Constants;
using Bookify.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    // we will switch to Hangfire / Quartz.NET for more complex scheduling needs
    // but for now this simple background service will run daily to check for expired subscriptions and downgrade users to the free plan if needed.
    public class SubscriptionWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SubscriptionWorker> _logger;

        public SubscriptionWorker(IServiceProvider serviceProvider, ILogger<SubscriptionWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                // Schedule next run at midnight UTC
                var nextRunTime = now.Date.AddDays(1);
                var delay = nextRunTime - now;

                _logger.LogInformation(
                    "Subscription Worker next run in {DelayHours:F2}h at {NextRunTime:u}",
                    delay.TotalHours, nextRunTime);

                // wait to allow the worker to run at the specified time
                await Task.Delay(delay, stoppingToken);

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        await ProcessSubscriptions(context, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during daily subscription processing.");
                }
            }
        }

        private async Task ProcessSubscriptions(AppDbContext context, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            // Fetch all active subscriptions that have expired
            var expiredSubs = await context.UserSubscriptions
                .Where(s => s.Status == AppConstants.SubscriptionStatus.Active && s.EndDate <= now)
                .Include(s => s.Plan)
                .ToListAsync(ct);

            if (!expiredSubs.Any()) return;

            // Downgrade expired subscriptions to the free plan
            var freePlan = await context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Code == AppConstants.PlanCode.FreeMonthly && p.IsActive, ct);
            using var transaction = await context.Database.BeginTransactionAsync(ct);
            try
            {


                foreach (var sub in expiredSubs)
                {
                    sub.Status = AppConstants.SubscriptionStatus.Expired;
                    sub.UpdatedAt = now;

                    // Re-subscribe user to free plan
                    if (freePlan != null)
                    {
                        var freeSub = new UserSubscription
                        {
                            UserId = sub.UserId,
                            PlanId = freePlan.Id,
                            IsTrial = false,
                            PriceAtPurchase = 0,
                            StartDate = now,
                            EndDate = now.AddYears(10), // Long-term free subscription
                            Status = AppConstants.SubscriptionStatus.Active,
                            CreatedAt = now
                        };
                        context.UserSubscriptions.Add(freeSub);
                    }

                    _logger.LogInformation("User {UserId} downgraded to free plan.", sub.UserId);
                }
                await context.SaveChangesAsync(ct);


                await transaction.CommitAsync(ct);

                _logger.LogInformation("Batch downgrade completed successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error during subscription downgrade. Rolling back changes.");

            }

        }
    }
}
