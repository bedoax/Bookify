using Bookify.Data;
using Bookify.Models.Constants;
using Bookify.Models.DTO.Subscription;
using Bookify.Models.Entities;
using Bookify.Models.Mapping;
using Bookify.Models.Roles;
using Bookify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Services.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly AppDbContext _context;
        private readonly IStripeService _stripeService;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(AppDbContext context, IStripeService stripeService, ILogger<SubscriptionService> logger)
        {
            _context = context;
            _stripeService = stripeService;
            _logger = logger;
        }
        public async Task<ServiceResponse<SubscriptionDto>> GetUserSubscriptionAsync(int userId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<SubscriptionDto>.Fail("Invalid user ID.");

            var userSubscription = await GetSubscriptionDto(userId, ct);
            if (userSubscription == null)
                return ServiceResponse<SubscriptionDto>.Fail("Subscription not found for this user.");

            return ServiceResponse<SubscriptionDto>.Ok(userSubscription);
        }

        public async Task<ServiceResponse<bool>> HasAccessToBookAsync(int userId, int bookId, CancellationToken ct = default)
        {
            if (userId <= 0) return ServiceResponse<bool>.Fail("Invalid user ID.");
            if (bookId <= 0) return ServiceResponse<bool>.Fail("Invalid book ID.");

            var hasAccess = await CheckHasAccessToBookAsync(userId, bookId, ct);

            if (!hasAccess)
                return ServiceResponse<bool>.Fail("User does not have access to this book.");

            return ServiceResponse<bool>.Ok(hasAccess, "User has access to this book.");
        }

        public async Task<ServiceResponse<SubscriptionDto>> SubscribeAsync(int userId, int planId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<SubscriptionDto>.Fail("Invalid userId");

            if (planId <= 0)
                return ServiceResponse<SubscriptionDto>.Fail("Invalid planId");

            var user = await GetUserAsync(userId, ct);
            if (user == null)
                return ServiceResponse<SubscriptionDto>.Fail("User Not Found");

            var planSubscription = await GetSubscriptionPlanAsync(planId, ct);
            if (planSubscription == null)
                return ServiceResponse<SubscriptionDto>.Fail("plan Not Found");

            if (string.IsNullOrEmpty(planSubscription.StripePriceId))
                return ServiceResponse<SubscriptionDto>.Fail("Stripe price not configured for this plan");
            using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                // NOTE: Stripe integration is active. The client must first create a
                // PaymentIntent via POST /api/payments/create-intent, collect the card
                // via Stripe.js, then call this endpoint after confirmation.
                var stripePayment = await _stripeService.ChargeCustomerAsync(user, planSubscription.StripePriceId!, ct);
                if (!stripePayment.Success)
                    return ServiceResponse<SubscriptionDto>.Fail("Payment failed: " + stripePayment.Message);

                var payment = CreatePayment(
                    planSubscription.Price,
                    AppConstants.Currency.USD,
                    AppConstants.PaymentStatus.Paid,
                    AppConstants.PaymentMethod.Stripe,
                    stripePayment.TransactionId);

                var userSubscription = CreateUserSubscription(
                    user.Id, planSubscription.Id, false,
                    planSubscription.Price, stripePayment.SubscriptionId,
                    planSubscription.DurationDays,
                    AppConstants.SubscriptionStatus.Active, payment);

                await _context.UserSubscriptions.AddAsync(userSubscription, ct);

                var history = CreateSubscriptionStatusHistory(
                    userSubscription.Id, "none",
                    AppConstants.SubscriptionStatus.Active,
                    AppConstants.ChangedBy.User);

                await _context.AddAsync(history, ct);
                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                var dto = CreateSubscriptionDto(
                    userId, userSubscription.PlanId,
                    planSubscription.Code, planSubscription.Name,
                    userSubscription.StartDate, userSubscription.EndDate,
                    AppConstants.SubscriptionStatus.Active, userSubscription.StripeSubscriptionId,
                    planSubscription.Price, userSubscription.IsTrial,
                    planSubscription.StripePriceId, planSubscription.DurationDays);

                return ServiceResponse<SubscriptionDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error subscribing user {UserId} to plan {PlanId}", userId, planId);
                return ServiceResponse<SubscriptionDto>.Fail("Failed to subscribe: " + ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> UnsubscribeAsync(int userId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid UserId");

            // جلب الاشتراك النشط فقط
            var userSubscription = await GetUserSubscription(userId, ct);
            if (userSubscription == null)
                return ServiceResponse<bool>.Fail("No active paid subscription found to unsubscribe.");

            var freePlan = await GetFreePlan();
            if (freePlan == null)
                return ServiceResponse<bool>.Fail("System Error: Free plan (FreeMonthly) not found in settings.");

            using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                // Cancel the recurring billing cycle in Stripe before downgrading
                // locally to prevent future unauthorized charges.
                if (!string.IsNullOrEmpty(userSubscription.StripeSubscriptionId))
                    await _stripeService.CancelSubscription(userSubscription.StripeSubscriptionId);

                userSubscription.PlanId = freePlan.Id;
                userSubscription.Status = AppConstants.SubscriptionStatus.Active;
                userSubscription.UpdatedAt = DateTime.UtcNow;
                userSubscription.EndDate = DateTime.UtcNow.AddDays(freePlan.DurationDays);
                userSubscription.IsTrial = false;
                userSubscription.PriceAtPurchase = null;
                userSubscription.StripeSubscriptionId = null;

                var history = CreateSubscriptionStatusHistory(
                    userSubscription.Id,
                    userSubscription.Status,
                    AppConstants.SubscriptionStatus.Active,
                    AppConstants.ChangedBy.User);

                await _context.AddAsync(history, ct);
                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return ServiceResponse<bool>.Ok(true, "Unsubscribed and moved to free plan successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error unsubscribing user {UserId}", userId);
                return ServiceResponse<bool>.Fail("Process failed: " + ex.Message);
            }
        }
        public async Task<ServiceResponse<bool>> StartFreeTrialAsync(int userId, CancellationToken ct = default)
        {
            if (userId <= 0)
                return ServiceResponse<bool>.Fail("Invalid userId");
            var isUsedFreeTrail = await IsUsedFreeTrail(userId, ct);
            if (isUsedFreeTrail)
                return ServiceResponse<bool>.Fail("User has already used the free trial before.");

            var premiumPlan = await GetPremuimPlan(ct);
            if (premiumPlan is null)
                return ServiceResponse<bool>.Fail("Premium plan not found. Please contact support.");
            try
            {


                var userSubscription = CreateUserSubscription(userId, premiumPlan.Id, true, 0, null, 7, "active", null);
                await _context.UserSubscriptions.AddAsync(userSubscription, ct);
                await _context.SaveChangesAsync(ct);
                return ServiceResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Fail("Failed to start free trial: " + ex.Message);
            }

        }
        private async Task<SubscriptionDto?> GetSubscriptionDto(int userId, CancellationToken ct)
        {
            return await _context.UserSubscriptions
                 .AsNoTracking()
                 .Where(x => x.UserId == userId)
                 .Select(MapToExpression.SubscriptionDto)
                 .FirstOrDefaultAsync(ct);

        }
        private async Task<bool> CheckHasAccessToBookAsync(int userId, int bookId, CancellationToken ct)
        {
            // 1. هات بيانات الكتاب (محتاجين الـ RequiredRank والـ IsFree)
            var book = await _context.Books
                    .AsNoTracking()
                    .Select(b => new { b.Id, b.IsFree, b.RequiredRank })
                    .FirstOrDefaultAsync(b => b.Id == bookId, ct);

            if (book == null) return false;


            if (book.IsFree || book.RequiredRank <= 0) return true;


            var userMaxRank = await _context.UserSubscriptions
                 .AsNoTracking()
                 .Where(us => us.UserId == userId &&
                  us.Status == "active" &&
                  us.EndDate >= DateTime.UtcNow)
                 .Select(us => (int?)us.Plan.Rank) // تحويل الرتبة لـ Nullable
                 .MaxAsync(ct) ?? 0;

            return userMaxRank >= book.RequiredRank;
        }
        private async Task<UserSubscription?> GetUserSubscription(int userId, CancellationToken ct)
        {
            return await _context.UserSubscriptions
                .FirstOrDefaultAsync(u => u.UserId == userId &&
                                         u.Status == AppConstants.SubscriptionStatus.Active, ct);
        }
        private SubscriptionStatusHistory CreateSubscriptionStatusHistory(int subscriptionId, string oldStatus, string newStatus, string changedBy)
        {
            return new SubscriptionStatusHistory
            {

                SubscriptionId = subscriptionId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow
            };
        }
        private async Task<SubscriptionPlan?> GetSubscriptionPlanAsync(int planId, CancellationToken ct)
        {
            return await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == planId && p.IsActive, ct);
        }

        private async Task<User?> GetUserAsync(int userId, CancellationToken ct)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
        }
        private Payment CreatePayment(decimal price, string currency, string status, string paymentMethod, string GatewayTxId)
        {
            return new Payment
            {
                Amount = price,
                Currency = currency,
                Status = status,
                PaymentMethod = paymentMethod,
                GatewayTxId = GatewayTxId,
                PaidAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
        }
        private UserSubscription CreateUserSubscription(int userId, int planId, bool isItTrial, decimal priceAtPurchase, string stripeSubscriptionId, double durationPlan, string status, Payment? payment)
        {
            var subscription = new UserSubscription
            {
                UserId = userId,
                PlanId = planId,
                IsTrial = isItTrial,
                PriceAtPurchase = priceAtPurchase,
                StripeSubscriptionId = stripeSubscriptionId,
                StartDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(durationPlan),
                Status = status,
                Payments = new List<Payment>()
            };


            if (payment != null)
            {
                subscription.Payments.Add(payment);
            }

            return subscription;
        }
        private SubscriptionDto CreateSubscriptionDto(int userId, int planId,
                                                     string planCode, string planName,
                                                     DateTime startDate, DateTime endDate,
                                                     string status, string stripeSubscriptionId,
                                                     decimal priceAtPurchase, bool isTrial,
                                                     string stripePriceId, int durationDays)
        {
            return new SubscriptionDto
            {
                UserId = userId,
                PlanId = planId,
                PlanCode = planCode,
                PlanName = planName,
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                StripeSubscriptionId = stripeSubscriptionId,
                PriceAtPurchase = priceAtPurchase,
                isTrial = isTrial,
                StripePriceId = stripePriceId,
                DurationDays = durationDays
            };
        }

        private async Task<SubscriptionPlan?> GetFreePlan()
        {
            return await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Code == AppConstants.PlanCode.FreeMonthly && p.IsActive);
        }
        public async Task<UserSubscription> CreateFreeSubscription(int userId)
        {
            var freePlan = await GetFreePlan();

            // If the free plan is not seeded in the DB, this is a configuration error — fail fast.
            // we can just return message instad of invalid operation exception but not now
            if (freePlan == null)
                throw new InvalidOperationException(
                    $"Free plan '{AppConstants.PlanCode.FreeMonthly}' not found. " +
                    "Ensure the database is seeded correctly.");

            return new UserSubscription
            {
                UserId = userId,
                PlanId = freePlan.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(freePlan.DurationDays),
                Status = AppConstants.SubscriptionStatus.Active,
                IsTrial = false,
                CreatedAt = DateTime.UtcNow
            };
        }
        private async Task<bool> IsUsedFreeTrail(int userId, CancellationToken ct)
        {
            return await _context.UserSubscriptions
                .AsNoTracking()
                .AnyAsync(us => us.UserId == userId && us.IsTrial, ct);
        }
        private async Task<SubscriptionPlan?> GetPremuimPlan(CancellationToken ct)
        {
            return await _context.SubscriptionPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == "PremiumMonthly" && p.IsActive, ct);

        }
    }
}








