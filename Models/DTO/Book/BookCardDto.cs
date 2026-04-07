namespace Bookify.Models.DTO.Book
{
    public class BookCardDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
        public List<string> AuthorNames { get; set; } = [];
        public bool IsFree { get; set; }
    }
}
