namespace Bookify.Models.Entities
{
    // ─────────────────────────────────────────────
    //  PROGRESS
    // ─────────────────────────────────────────────

    public class UserBookProgress
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int? LastReadPage { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Optimistic concurrency token (SQL Server rowversion).
        /// On DbUpdateConcurrencyException: resolve by keeping the HIGHER page number,
        /// not last-write-wins.
        /// </summary>
        public byte[] RowVersion { get; set; } = null!;
    }
}
