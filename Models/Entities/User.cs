namespace Bookify.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;       
        public string Email { get; set; } = null!;           
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "member";        

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
        public ICollection<UserSubscription> Subscriptions { get; set; } = [];
        public ICollection<Collection> Collections { get; set; } = [];
        public ICollection<FavoriteBook> FavoriteBooks { get; set; } = [];
        public ICollection<UserBookProgress> BookProgress { get; set; } = [];
        public ICollection<UserAudioBookProgress> AudioProgress { get; set; } = [];
        public ICollection<BookReview> BookFeedbacks { get; set; } = [];
        public ICollection<Notification> Notifications { get; set; } = [];
        public ICollection<ActivityLog> ActivityLogs { get; set; } = [];
        public ICollection<PasswordResetCode> PasswordResetCodes { get; set; } = [];
    }
}
