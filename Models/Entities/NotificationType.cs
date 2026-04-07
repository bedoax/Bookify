namespace Bookify.Models.Entities
{
    // ─────────────────────────────────────────────
    //  NOTIFICATIONS
    // ─────────────────────────────────────────────

    public class NotificationType
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;           // MaxLength(100) → Fluent
        public string? Description { get; set; }

        public ICollection<Notification> Notifications { get; set; } = [];
    }
}
