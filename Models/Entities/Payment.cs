namespace Bookify.Models.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        /// <summary>
        /// Subscriptions are never hard-deleted (billing audit trail).
        /// OnDelete = Restrict. SetNull has no practical use here.
        /// </summary>
        public int? SubscriptionId { get; set; }
        public UserSubscription? Subscription { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";       // MaxLength(3)   → Fluent
        public string Status { get; set; } = "pending";     // MaxLength(20)  → Fluent
        public string? PaymentMethod { get; set; }          // MaxLength(100) → Fluent

        /// <summary>Stripe PaymentIntent / Charge ID — idempotency key</summary>
        public string? GatewayTxId { get; set; }            // MaxLength(255) → Fluent

        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
