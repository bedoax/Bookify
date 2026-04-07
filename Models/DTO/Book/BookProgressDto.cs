namespace Bookify.Models.DTO.Book
{
    public class BookProgressDto
    {

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        // calculated as (CurrentPage / TotalPages) * 100 in the service layer in a function
        public double ProgressPercentage { get; set; }
        // When the user last read this book
        public DateTime LastAccessedAt { get; set; }
    }
}
