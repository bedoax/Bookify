namespace Bookify.Models.DTO.Review
{
    public class ReviewDto:IReview
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public int Rating { get; set; }
        public string? ReviewContent { get; set; }
    }
}
