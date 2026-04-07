namespace Bookify.Models.Entities
{
    public class BookFile
    {
        public int Id { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        /// <summary>
        /// Internal object-storage key (e.g. "books/42/file.epub").
        /// Never expose to clients — generate signed URLs in service layer.
        /// </summary>
        public string StorageKey { get; set; } = null!;     // MaxLength(500) → Fluent

        /// <summary>
        /// Optional cached CDN URL. Can be null if always generated on-the-fly.
        /// </summary>
        public string? CdnUrl { get; set; }                 // MaxLength(500) → Fluent

        /// <summary>PDF | EPUB | MP3 | M4B</summary>
        public string FileFormat { get; set; } = null!;     // MaxLength(10)  → Fluent

        public decimal? FileSizeMb { get; set; }

        /// <summary>
        /// Seconds — valid only for MP3/M4B.
        /// Must be NULL for PDF/EPUB — enforced in application validation layer.
        /// </summary>
        public int? PageCount { get; set; }
        public int? AudioDuration { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<BookFileNarrator> BookFileNarrators { get; set; } = [];
    }
}
