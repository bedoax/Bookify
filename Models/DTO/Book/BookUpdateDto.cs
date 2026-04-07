namespace Bookify.Models.DTO.Book
{
    public class BookUpdateDto
    {
        // We exclude the ID here because it's typically in the URL route

        public string? Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool? IsFree { get; set; }
        public DateTime? PublishDate { get; set; }
        public int? RequiredRank { get; set; }

        // Updating these will replace the old links in BookAuthors/BookTags
        public List<int>? AuthorIds { get; set; } = null;
        public List<int>? TagIds { get; set; } = null;
        public int? Rank { get; set; }
        public string? PdfUrl { get; set; }
        public string? AudioUrl { get; set; }
        public int? PageCount { get; set; }
        public TimeSpan? AudioDuration { get; set; }
    }
}
