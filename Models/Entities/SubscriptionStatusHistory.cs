namespace Bookify.Models.Entities
{
    /// <summary>
    /// Immutable audit log — one row per status transition.
    /// Never delete rows from this table.
    /// </summary>
    public class SubscriptionStatusHistory
    {
        public int Id { get; set; }

        public int SubscriptionId { get; set; }
        public UserSubscription Subscription { get; set; } = null!;

        public string OldStatus { get; set; } = null!;      // MaxLength(20) → Fluent
        public string NewStatus { get; set; } = null!;      // MaxLength(20) → Fluent

        /// <summary>"user" | "system" | "admin:{id}"</summary>
        public string ChangedBy { get; set; } = "system";   // MaxLength(50) → Fluent

        public DateTime ChangedAt { get; set; }
    }
}
