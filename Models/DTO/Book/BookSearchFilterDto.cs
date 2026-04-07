namespace Bookify.Models.DTO.Book
{
    public class BookSearchFilterDto
    {
        public string? SearchTerm { get; set; }
        public List<int>? TagIds { get; set; }
        public int? AuthorId { get; set; }
        public bool? IsFree { get; set; }
        public DateTime? PublishedFrom { get; set; }
        public DateTime? PublishedTo { get; set; }
        public string? FileFormat { get; set; } // "pdf" or "mp3"
        public string? SortBy { get; set; } // "newest", "alphabetical", "popular"
    }
}