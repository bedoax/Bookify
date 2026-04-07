namespace Bookify.Models.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }          // MaxLength(500) → Fluent

        /// <summary>true = accessible without subscription</summary>
        public bool IsFree { get; set; }
        public int RequiredRank { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<BookFile> Files { get; set; } = [];
        public ICollection<BookAuthor> BookAuthors { get; set; } = [];
        public ICollection<BookTag> BookTags { get; set; } = [];
        public ICollection<CollectionBook> CollectionBooks { get; set; } = [];
        public ICollection<FavoriteBook> FavoritedBy { get; set; } = [];
        public ICollection<UserBookProgress> ReadersProgress { get; set; } = [];
        public ICollection<UserAudioBookProgress> ListenersProgress { get; set; } = [];
        public ICollection<BookReview> Feedbacks { get; set; } = [];
        public ICollection<ActivityLog> ActivityLogs { get; set; } = [];
    }
}
