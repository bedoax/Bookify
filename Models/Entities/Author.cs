namespace Bookify.Models.Entities
{
    // ─────────────────────────────────────────────
    //  BOOKS & CONTENT
    // ─────────────────────────────────────────────

    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Bio { get; set; }

        /// <summary>DateOnly → SQL DATE column (not datetime2)</summary>
        public DateOnly? Birthdate { get; set; }
        public DateOnly? Deathdate { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<BookAuthor> BookAuthors { get; set; } = [];
    }
}
