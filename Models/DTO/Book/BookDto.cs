namespace Bookify.Models.DTO.Book
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public int RequiredRank { get; set; }

        // --- Critical for UI Performance ---
        // Now the frontend can show "By: Stephen King" without another API call
        public List<int> AuthorIds { get; set; } = [];
        public List<string> AuthorNames { get; set; } = [];

        public List<int> TagIds { get; set; } = [];
        public List<string> TagNames { get; set; } = [];

        // --- Critical for your Access Rules ---
        public bool IsFree { get; set; }

        // --- Content & Metadata ---
        public string? PdfUrl { get; set; }
        public string? AudioUrl { get; set; }
        public int? PageCount { get; set; }
        public TimeSpan? AudioDuration { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool CanAccess { get; set; }
    }
}
