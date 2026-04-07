
using Bookify.Models.DTO.Subscription;
using Bookify.Models.Entities;
using System.Linq.Expressions;

namespace Bookify.Models.Mapping
{
    public static class MapToExpression
    {
        public static Expression<Func<UserSubscription, SubscriptionDto>> SubscriptionDto
            => x => new SubscriptionDto
            {
                StartDate = x.StartDate,
                DurationDays = x.Plan.DurationDays,
                EndDate = x.EndDate,
                isTrial = x.IsTrial,
                PriceAtPurchase = x.PriceAtPurchase,
                UserId = x.UserId,
                PlanCode = x.Plan.Code,
                PlanName = x.Plan.Name,
                Status = x.Status,
                StripeSubscriptionId = x.StripeSubscriptionId,
                PlanId = x.PlanId,
                StripePriceId = x.Plan.StripePriceId
            };
    }

}

