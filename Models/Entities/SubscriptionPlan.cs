namespace Bookify.Models.Entities
{
    // ─────────────────────────────────────────────
    //  SUBSCRIPTIONS & BILLING
    // ─────────────────────────────────────────────

    public class SubscriptionPlan
    {
        public int Id { get; set; }

        /// <summary>Stable internal code — never changes (e.g. "premium_monthly")</summary>
        public string Code { get; set; } = null!;           // MaxLength(100) → Fluent

        /// <summary>Display name shown in UI</summary>
        public string Name { get; set; } = null!;           // MaxLength(100) → Fluent
        public int Rank { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public bool IsActive { get; set; } = true;

        /// <summary>Stripe Price ID — optional</summary>
        public string? StripePriceId { get; set; }          // MaxLength(255) → Fluent

        public DateTime CreatedAt { get; set; }

        public ICollection<UserSubscription> Subscriptions { get; set; } = [];
      
    }
}
