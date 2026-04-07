namespace Bookify.Models.Entities
{
    // ─────────────────────────────────────────────
    //  FEEDBACK
    // ─────────────────────────────────────────────

    public class BookReview
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        /// <summary>1–5</summary>
        public int Rating { get; set; }

        public string? ReviewContent { get; set; }

        /// <summary>Flagged for moderator review</summary>
        public bool IsFlagged { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
