namespace Bookify.Models.Entities
{
    // ─────────────────────────────────────────────
    //  JUNCTION TABLES
    // ─────────────────────────────────────────────

    public class BookAuthor
    {
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int AuthorId { get; set; }
        public Author Author { get; set; } = null!;

        /// <summary>author | co_author | editor | translator</summary>
        public string Role { get; set; } = "author";        // MaxLength(20) → Fluent
        public int SortOrder { get; set; } = 1;
    }
}
