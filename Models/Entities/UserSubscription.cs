namespace Bookify.Models.Entities
{
    public class UserSubscription
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int PlanId { get; set; }
        public SubscriptionPlan Plan { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsTrial { get; set; }

        /// <summary>active | cancelled | expired | paused</summary>
        public string Status { get; set; } = "active";      // MaxLength(20) → Fluent

        /// <summary>Price snapshot at purchase — plan price may change later</summary>
        public decimal? PriceAtPurchase { get; set; }

        /// <summary>Stripe recurring subscription ID</summary>
        public string? StripeSubscriptionId { get; set; }   // MaxLength(255) → Fluent

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Payment> Payments { get; set; } = [];
        public ICollection<SubscriptionStatusHistory> StatusHistory { get; set; } = [];
    }
}
