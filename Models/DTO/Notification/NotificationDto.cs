namespace Bookify.Models.DTO.Notification
{
    public class NotificationDto
    {
        // public string Code { get; set; } = null!;
        public int Id { get; set; } 
        public string Message { get; set; } = null!;
        public bool ReadStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
