namespace Bookify.Models.DTO.Review
{
    public interface IReview
    {
        public int BookId { get; set; }
        public int Rating { get; set; }
        public string? ReviewContent { get; set; }
    }
}
