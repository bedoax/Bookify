using Bookify.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
namespace Bookify.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // AUTH
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // SUBSCRIPTIONS & BILLING
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
        public DbSet<SubscriptionStatusHistory> SubscriptionStatusHistory => Set<SubscriptionStatusHistory>();
        public DbSet<Payment> Payments => Set<Payment>();

        // BOOKS & CONTENT
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Book> Books => Set<Book>();
        public DbSet<BookFile> BookFiles => Set<BookFile>();
        public DbSet<Narrator> Narrators => Set<Narrator>();
        public DbSet<BookFileNarrator> BookFileNarrators => Set<BookFileNarrator>();
        public DbSet<Tag> Tags => Set<Tag>();

        // JUNCTIONS
        public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
        public DbSet<BookTag> BookTags => Set<BookTag>();

        // COLLECTIONS & FAVORITES
        public DbSet<Collection> Collections => Set<Collection>();
        public DbSet<CollectionBook> CollectionBooks => Set<CollectionBook>();
        public DbSet<FavoriteBook> FavoriteBooks => Set<FavoriteBook>();

        // PROGRESS
        public DbSet<UserBookProgress> UserBookProgress => Set<UserBookProgress>();
        public DbSet<UserAudioBookProgress> UserAudioBookProgress => Set<UserAudioBookProgress>();

        // FEEDBACK
        public DbSet<BookReview> BookReview => Set<BookReview>();

        // NOTIFICATIONS
        public DbSet<NotificationType> NotificationTypes => Set<NotificationType>();
        public DbSet<Notification> Notifications => Set<Notification>();

        // ACTIVITY LOGS
        public DbSet<ActionType> ActionTypes => Set<ActionType>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

        //Rest Password
        public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
