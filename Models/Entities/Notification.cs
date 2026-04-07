namespace Bookify.Models.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int NotificationTypeId { get; set; }
        public NotificationType NotificationType { get; set; } = null!;

        public string Message { get; set; } = null!;
        public string? ActionUrl { get; set; }              // MaxLength(500) → Fluent
        public bool ReadStatus { get; set; }

        /// <summary>
        /// Optional TTL. NULL = never expires.
        /// Background job uses IX_notif_expires_at to purge stale rows.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
