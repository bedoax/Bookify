namespace Bookify.Models.DTO.Review
{
    public class ReviewCreateDto : IReview
    {

        // don't forget to do the is flaged for modirator edit
        public int BookId { get; set; }
        public int Rating { get; set; }
        public string? ReviewContent { get; set; }
    }
}
