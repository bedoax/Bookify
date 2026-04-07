namespace Bookify.Models.Entities
{
    // ─────────────────────────────────────────────
    //  ACTIVITY LOGS
    // ─────────────────────────────────────────────

    public class ActionType
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;           // MaxLength(100) → Fluent
        public string? Description { get; set; }

        public ICollection<ActivityLog> ActivityLogs { get; set; } = [];
    }
}
