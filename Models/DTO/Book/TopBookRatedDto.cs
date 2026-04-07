namespace Bookify.Models.DTO.Book
{
    public class TopBookRatedDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
    }
}
