using Bookify.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> e)
    {
        e.ToTable("users");
        e.HasKey(x => x.Id);

        e.Property(x => x.Username).HasMaxLength(100);
        e.Property(x => x.Email).HasMaxLength(255);
        e.Property(x => x.Role)
            .HasMaxLength(20)
            .HasDefaultValue("user");

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        e.HasCheckConstraint("CK_users_role", "[Role] IN ('user','admin','moderator')");

        // Filtered unique indexes — soft delete aware
        e.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("UQ_users_email_active")
            .HasFilter("[DeletedAt] IS NULL");

        e.HasIndex(x => x.Username)
            .IsUnique()
            .HasDatabaseName("UQ_users_username_active")
            .HasFilter("[DeletedAt] IS NULL");

        e.HasIndex(x => x.DeletedAt)
            .HasDatabaseName("IX_users_deleted_at");

        // ⚠️ NO global query filter on User intentionally.
        // Filtering soft-deleted users at the DB level causes silent data loss
        // in admin tools, token cleanup jobs, and any query that navigates to User.
        // Instead, apply .Where(x => x.DeletedAt == null) explicitly in repositories.
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> e)
    {
        e.ToTable("refresh_tokens");
        e.HasKey(x => x.Id);

        e.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // SHA-256 hex = always 64 chars — critical for index performance
        e.Property(x => x.TokenHash).HasMaxLength(64);

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        e.HasOne(x => x.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasIndex(x => x.TokenHash)
            .IsUnique()
            .HasDatabaseName("UQ_refresh_tokens_token_hash");

        e.HasCheckConstraint(
            "CK_refresh_tokens_revoked",
            "(([Revoked] = 0 AND [RevokedAt] IS NULL) OR ([Revoked] = 1 AND [RevokedAt] IS NOT NULL))"
        );
    }
}

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> e)
    {
        e.ToTable("subscription_plans");
        e.HasKey(x => x.Id);

        e.Property(x => x.Code).HasMaxLength(100);
        e.Property(x => x.Name).HasMaxLength(100);
        e.Property(x => x.StripePriceId).HasMaxLength(255);

        // ✅ Fix decimal warning
        e.Property(x => x.Price).HasPrecision(18, 2);

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        e.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("UQ_subscription_plans_code");

        e.HasIndex(x => x.Name)
            .HasDatabaseName("IX_subscription_plans_name");

        e.HasIndex(x => x.StripePriceId)
            .IsUnique()
            .HasDatabaseName("UQ_subscription_plans_stripe_price_id")
            .HasFilter("[StripePriceId] IS NOT NULL");

        // ✅ FIX: use real column names (PascalCase)
        e.HasCheckConstraint("CK_plans_price", "[Price] >= 0");
        e.HasCheckConstraint("CK_plans_duration", "[DurationDays] > 0");
    }
}

public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> e)
    {
        e.ToTable("user_subscriptions");
        e.HasKey(x => x.Id);

        e.Property(x => x.Status)
            .HasMaxLength(20)
            .HasDefaultValue("active");

        e.Property(x => x.StripeSubscriptionId).HasMaxLength(255);

        // ✅ Fix decimal warning
        e.Property(x => x.PriceAtPurchase).HasPrecision(18, 2);

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        e.HasOne(x => x.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Plan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(x => new { x.UserId, x.Status })
            .HasDatabaseName("IX_usub_user_status");

        e.HasIndex(x => x.EndDate)
            .HasDatabaseName("IX_usub_end_date");

        e.HasIndex(x => x.StripeSubscriptionId)
            .IsUnique()
            .HasDatabaseName("UQ_usub_stripe_subscription_id")
            .HasFilter("[StripeSubscriptionId] IS NOT NULL");

        // One active subscription per user at a time.
        // ⚠️ Business decision: if paused = access still granted,
        // change filter to: "[Status] IN ('active','paused')"
        e.HasIndex(x => x.UserId)
            .IsUnique()
            .HasDatabaseName("UX_user_one_active_sub")
            .HasFilter("[Status] = 'active'");

        e.HasCheckConstraint("CK_user_subscriptions_status",
            "[Status] IN ('active','cancelled','expired','paused')");

        e.HasCheckConstraint("CK_user_subscriptions_dates", "[EndDate] > [StartDate]");
    }
}

public class SubscriptionStatusHistoryConfiguration : IEntityTypeConfiguration<SubscriptionStatusHistory>
{
    public void Configure(EntityTypeBuilder<SubscriptionStatusHistory> e)
    {
        e.ToTable("subscription_status_history");
        e.HasKey(x => x.Id);

        e.Property(x => x.OldStatus).HasMaxLength(20);
        e.Property(x => x.NewStatus).HasMaxLength(20);
        e.Property(x => x.ChangedBy).HasMaxLength(50).HasDefaultValue("system");

        e.Property(x => x.ChangedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        e.HasOne(x => x.Subscription)
            .WithMany(s => s.StatusHistory)
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasIndex(x => new { x.SubscriptionId, x.ChangedAt })
            .HasDatabaseName("IX_sub_history_sub_date");

        // Validate that status values in history match allowed subscription statuses
        e.HasCheckConstraint("CK_sub_history_old_status",
            "[OldStatus] IN ('active','cancelled','expired','paused')");
        e.HasCheckConstraint("CK_sub_history_new_status",
            "[NewStatus] IN ('active','cancelled','expired','paused')");
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> e)
    {
        e.ToTable("payments");
        e.HasKey(x => x.Id);

        e.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("USD");
        e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("pending");
        e.Property(x => x.PaymentMethod).HasMaxLength(100);
        e.Property(x => x.GatewayTxId).HasMaxLength(255);

        // ✅ Fix decimal warning
        e.Property(x => x.Amount).HasPrecision(18, 2);

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        // Restrict: subscriptions are never deleted — this FK should never fire.
        // Using Restrict makes any accidental hard-delete of a subscription fail loudly.
        e.HasOne(x => x.Subscription)
            .WithMany(s => s.Payments)
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(x => x.SubscriptionId)
            .HasDatabaseName("IX_payments_subscription_id");

        e.HasIndex(x => x.GatewayTxId)
            .IsUnique()
            .HasDatabaseName("UQ_payments_gateway_tx_id")
            .HasFilter("[GatewayTxId] IS NOT NULL");

        e.HasCheckConstraint("CK_payments_status",
            "[Status] IN ('pending','completed','failed','refunded')");
        e.HasCheckConstraint("CK_payments_amount", "[Amount] >= 0");
    }
}

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> e)
    {
        e.ToTable("authors");
        e.HasKey(x => x.Id);

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        // DateOnly → SQL DATE (not datetime2)
        e.Property(x => x.Birthdate).HasColumnType("date");
        e.Property(x => x.Deathdate).HasColumnType("date");

        // ⚠️ No global query filter — see User config for explanation.
        // Apply .Where(x => x.DeletedAt == null) in repositories.
    }
}

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> e)
    {
        e.ToTable("books");
        e.HasKey(x => x.Id);

        e.Property(x => x.CoverImageUrl).HasMaxLength(500);

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        e.HasIndex(x => x.DeletedAt).HasDatabaseName("IX_books_deleted_at");
        e.HasIndex(x => x.PublishedAt).HasDatabaseName("IX_books_published_at");
        e.HasIndex(x => x.IsFree).HasDatabaseName("IX_books_is_free");

        // ⚠️ No global query filter — see User config for explanation.
        // Apply .Where(x => x.DeletedAt == null) in repositories.
    }
}

public class BookFileConfiguration : IEntityTypeConfiguration<BookFile>
{
    public void Configure(EntityTypeBuilder<BookFile> e)
    {
        e.ToTable("book_files");
        e.HasKey(x => x.Id);

        e.Property(x => x.StorageKey).HasMaxLength(500);
        e.Property(x => x.CdnUrl).HasMaxLength(500);
        e.Property(x => x.FileFormat).HasMaxLength(10);

        // ✅ Fix decimal warning
        e.Property(x => x.FileSizeMb).HasPrecision(10, 2);

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        e.HasOne(x => x.Book)
            .WithMany(b => b.Files)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasCheckConstraint("CK_book_files_format",
            "[FileFormat] IN ('PDF','EPUB','MP3','M4B')");
        e.HasCheckConstraint("CK_book_files_size",
            "[FileSizeMb] IS NULL OR [FileSizeMb] >= 0");
        e.HasCheckConstraint("CK_book_files_duration",
            "[AudioDuration] IS NULL OR [AudioDuration] >= 0");
        // ⚠️ AudioDuration vs FileFormat cross-validation is enforced in the
        // application validation layer (FluentValidation / service layer),
        // not in DB — composite CHECK with CASE is fragile in EF migrations.
    }
}

public class NarratorConfiguration : IEntityTypeConfiguration<Narrator>
{
    public void Configure(EntityTypeBuilder<Narrator> e)
    {
        e.ToTable("narrators");
        e.HasKey(x => x.Id);

        e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        e.Property(x => x.Bio).IsRequired(false).HasMaxLength(1000);
        e.Property(x => x.ImageUrl).IsRequired(false).HasMaxLength(500);
    }
}

public class BookFileNarratorConfiguration : IEntityTypeConfiguration<BookFileNarrator>
{
    public void Configure(EntityTypeBuilder<BookFileNarrator> e)
    {
        e.ToTable("book_file_narrators"); // اسم الجدول الجديد للوسيط
        e.HasKey(x => new { x.BookFileId, x.NarratorId }); // composite key

        e.HasOne(x => x.BookFile)
         .WithMany(bf => bf.BookFileNarrators)
         .HasForeignKey(x => x.BookFileId)
         .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Narrator)
         .WithMany(n => n.BookFileNarrators)
         .HasForeignKey(x => x.NarratorId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}


public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> e)
    {
        e.ToTable("tags");

        e.HasKey(x => x.Id);
        
        e.Property(x => x.Name)
            .HasMaxLength(100);
          

        e.HasIndex(x => x.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UQ_tags_name");

        e.HasQueryFilter(t => !t.IsDeleted);
    }
}


public class BookAuthorConfiguration : IEntityTypeConfiguration<BookAuthor>
{
    public void Configure(EntityTypeBuilder<BookAuthor> e)
    {
        e.ToTable("book_authors");
        e.HasKey(x => new { x.BookId, x.AuthorId });

        e.Property(x => x.Role).HasMaxLength(20).HasDefaultValue("author");
        e.Property(x => x.SortOrder).HasDefaultValue(1);

        e.HasOne(x => x.Book)
            .WithMany(b => b.BookAuthors)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Author)
            .WithMany(a => a.BookAuthors)
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasCheckConstraint("CK_book_authors_role",
            "[Role] IN ('author','co_author','editor','translator')");
    }
}

public class BookTagConfiguration : IEntityTypeConfiguration<BookTag>
{
    public void Configure(EntityTypeBuilder<BookTag> e)
    {
        e.ToTable("book_tags");
        e.HasKey(x => new { x.BookId, x.TagId });

        e.HasOne(x => x.Book)
            .WithMany(b => b.BookTags)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Tag)
            .WithMany(t => t.BookTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasIndex(x => x.TagId)
            .HasDatabaseName("IX_book_tags_tag_id");
    }
}



public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> e)
    {
        e.ToTable("collections");
        e.HasKey(x => x.Id);

        e.Property(x => x.Name).HasMaxLength(255);

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        e.HasOne(x => x.User)
            .WithMany(u => u.Collections)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasIndex(x => new { x.UserId, x.Name })
            .IsUnique()
            .HasDatabaseName("UQ_collections_user_name");
    }
}

public class CollectionBookConfiguration : IEntityTypeConfiguration<CollectionBook>
{
    public void Configure(EntityTypeBuilder<CollectionBook> e)
    {
        e.ToTable("collection_books");
        e.HasKey(x => new { x.CollectionId, x.BookId });

        e.Property(x => x.AddedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        e.HasOne(x => x.Collection)
            .WithMany(c => c.CollectionBooks)
            .HasForeignKey(x => x.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Book)
            .WithMany(b => b.CollectionBooks)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FavoriteBookConfiguration : IEntityTypeConfiguration<FavoriteBook>
{
    public void Configure(EntityTypeBuilder<FavoriteBook> e)
    {
        e.ToTable("favorite_books");
        e.HasKey(x => new { x.UserId, x.BookId });

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        e.HasOne(x => x.User)
            .WithMany(u => u.FavoriteBooks)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Book)
            .WithMany(b => b.FavoritedBy)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserBookProgressConfiguration : IEntityTypeConfiguration<UserBookProgress>
{
    public void Configure(EntityTypeBuilder<UserBookProgress> e)
    {
        e.ToTable("user_book_progress");
        e.HasKey(x => new { x.UserId, x.BookId });

        // Optimistic concurrency — resolve conflicts in service layer:
        // keep the HIGHER LastReadPage value, not last-write-wins
        e.Property(x => x.RowVersion).IsRowVersion();

        e.HasOne(x => x.User)
            .WithMany(u => u.BookProgress)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Book)
            .WithMany(b => b.ReadersProgress)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ FIX: use real column/property name
        e.HasCheckConstraint(
            "CK_user_book_progress_nonneg",
            "[LastReadPage] IS NULL OR [LastReadPage] >= 0"
        );
    }
}

public class UserAudioBookProgressConfiguration : IEntityTypeConfiguration<UserAudioBookProgress>
{
    public void Configure(EntityTypeBuilder<UserAudioBookProgress> e)
    {
        e.ToTable("user_audio_book_progress");
        e.HasKey(x => new { x.UserId, x.BookId });

        // Optimistic concurrency — resolve conflicts in service layer:
        // keep the HIGHER LastListenSecond value, not last-write-wins
        e.Property(x => x.RowVersion).IsRowVersion();

        e.HasOne(x => x.User)
            .WithMany(u => u.AudioProgress)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Book)
            .WithMany(b => b.ListenersProgress)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasCheckConstraint("CK_user_audio_progress_nonneg",
            "[LastListenSecond] IS NULL OR [LastListenSecond] >= 0");
    }
}

public class BookFeedbackConfiguration : IEntityTypeConfiguration<BookReview>
{
    public void Configure(EntityTypeBuilder<BookReview> e)
    {
        e.ToTable("book_feedback");
        e.HasKey(x => x.Id);

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        e.HasOne(x => x.User)
            .WithMany(u => u.BookFeedbacks)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Book)
            .WithMany(b => b.Feedbacks)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // One review per user per book
        e.HasIndex(x => new { x.UserId, x.BookId })
            .IsUnique()
            .HasDatabaseName("UQ_book_feedback");

        e.HasIndex(x => x.BookId)
            .HasDatabaseName("IX_feedback_book_id");

        e.HasCheckConstraint("CK_feedback_rating", "[Rating] BETWEEN 1 AND 5");
    }
}

public class NotificationTypeConfiguration : IEntityTypeConfiguration<NotificationType>
{
    public void Configure(EntityTypeBuilder<NotificationType> e)
    {
        e.ToTable("notification_types");
        e.HasKey(x => x.Id);

        e.Property(x => x.Code).HasMaxLength(100);

        e.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("UQ_notification_types_code");
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> e)
    {
        e.ToTable("notifications");
        e.HasKey(x => x.Id);

        e.Property(x => x.ActionUrl).HasMaxLength(500);
        e.Property(x => x.ReadStatus).HasDefaultValue(false);

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        e.HasOne(x => x.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.NotificationType)
            .WithMany(t => t.Notifications)
            .HasForeignKey(x => x.NotificationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(x => new { x.UserId, x.ReadStatus })
            .HasDatabaseName("IX_notif_user_read");

        // Used by background cleanup job to purge expired notifications
        e.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("IX_notif_expires_at")
            .HasFilter("[ExpiresAt] IS NOT NULL");
    }
}

public class ActionTypeConfiguration : IEntityTypeConfiguration<ActionType>
{
    public void Configure(EntityTypeBuilder<ActionType> e)
    {
        e.ToTable("action_types");
        e.HasKey(x => x.Id);

        e.Property(x => x.Code).HasMaxLength(100);

        e.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("UQ_action_types_code");
    }
}

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> e)
    {
        e.ToTable("activity_logs");
        e.HasKey(x => x.Id);

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        e.HasOne(x => x.User)
            .WithMany(u => u.ActivityLogs)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.ActionType)
            .WithMany(a => a.ActivityLogs)
            .HasForeignKey(x => x.ActionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: log is preserved even if the referenced book is soft-deleted
        e.HasOne(x => x.Book)
            .WithMany(b => b.ActivityLogs)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.SetNull);

        e.HasIndex(x => new { x.UserId, x.CreatedAt })
            .HasDatabaseName("IX_actlog_user_created");
    }

    public class PasswordResetCodeConfiguration : IEntityTypeConfiguration<PasswordResetCode>
    {
        public void Configure(EntityTypeBuilder<PasswordResetCode> e)
        {
            e.ToTable("password_reset_codes");

            e.HasKey(x => x.Id);

            e.Property(x => x.CodeHash)
                .IsRequired()
                .HasMaxLength(255);

            e.Property(x => x.ExpiresAt)
                .IsRequired();

            e.Property(x => x.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            e.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(x => x.User)
                .WithMany(u => u.PasswordResetCodes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.ExpiresAt);
        }
    }
}