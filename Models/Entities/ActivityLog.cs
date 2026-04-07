namespace Bookify.Models.Entities
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ActionTypeId { get; set; }
        public ActionType ActionType { get; set; } = null!;

        /// <summary>Nullable — log survives if the book is soft-deleted</summary>
        public int? BookId { get; set; }
        public Book? Book { get; set; }

        /// <summary>JSON blob for any extra context</summary>
        public string? Metadata { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
