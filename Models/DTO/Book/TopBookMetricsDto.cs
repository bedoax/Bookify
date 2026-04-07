namespace Bookify.Models.DTO.Book
{
    public class TopBookMetricsDto
    {
       public int BookId { get; set; }
       public string Title { get; set; }
        public string? CoverImageUrl { get; set; }
        public int Count { get; set; }

        // can be Reads , Favorites , Listens 
        public string MetricLabel { get; set; }
    }
}
