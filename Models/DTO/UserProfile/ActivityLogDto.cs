namespace Bookify.Models.DTO.UserProfile
{
    public class ActivityLogDto
    {
        //from user
        public string UserName { get; set; } = null!;
        //from activity log
        public string ActivityCode { get; set; } = null!;
        public string? ActivityDescription { get; set; }
        public string? Metadata { get; set; }
        // from book 
        public string? BookTitle { get; set; } = null!;


    }
}
