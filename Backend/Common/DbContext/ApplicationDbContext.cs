using Backend.Common.DbContext.Chat;
using Backend.Common.DbContext.Post;
using Backend.Common.DbContext.Reaction;
using Backend.Common.Helpers.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext;

public partial class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chat.Chat> Chats { get; set; }

    public virtual DbSet<ChatParticipant> ChatParticipants { get; set; }
    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Follow> Follows { get; set; }

    public virtual DbSet<Hashtag> Hashtags { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<Medium> Media { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<MessageMedium> MessageMedia { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Post.Post> Posts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSetting> UserSettings { get; set; }

    public virtual DbSet<PostReaction> PostReactions { get; set; }

    public virtual DbSet<CommentReaction> CommentReactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat.Chat>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("chats_pkey");

            entity.ToTable("chats");

            entity.Property(e => e.ChatId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("chat_id");
            entity.Property(e => e.ChatName)
                .HasMaxLength(100)
                .HasColumnName("chat_name");
            entity.Property(e => e.ChatType)
                .HasMaxLength(20)
                .HasColumnName("chat_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.MessageCount)
                .HasDefaultValue(0)
                .HasColumnName("message_count");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<ChatParticipant>(entity =>
        {
            entity.HasKey(e => new { e.ChatId, e.UserId }).HasName("chat_participants_pkey");

            entity.ToTable("chat_participants");

            entity.HasIndex(e => e.UserId, "idx_chat_participants_user_id");

            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("joined_at");
            entity.Property(e => e.LastReadMessageId).HasColumnName("last_read_message_id");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValueSql("'member'::character varying")
                .HasColumnName("role");

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatParticipants)
                .HasForeignKey(d => d.ChatId)
                .HasConstraintName("chat_participants_chat_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ChatParticipants)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("chat_participants_user_id_fkey");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("comments_pkey");

            entity.ToTable("comments");

            entity.HasIndex(e => e.ParentCommentId, "idx_comments_parent_comment_id");

            entity.HasIndex(e => e.PostId, "idx_comments_post_id");

            entity.HasIndex(e => e.UserId, "idx_comments_user_id");

            entity.Property(e => e.CommentId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("comment_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment)
                .HasForeignKey(d => d.ParentCommentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("comments_parent_comment_id_fkey");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("comments_post_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("comments_user_id_fkey");
        });

        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasKey(e => e.FollowId).HasName("follows_pkey");

            entity.ToTable("follows");

            entity.HasIndex(e => new { e.FollowerId, e.FollowingId }, "follows_follower_id_following_id_key")
                .IsUnique();

            entity.HasIndex(e => e.FollowerId, "idx_follows_follower_id");

            entity.HasIndex(e => e.FollowingId, "idx_follows_following_id");

            entity.Property(e => e.FollowId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("follow_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.FollowerId).HasColumnName("follower_id");
            entity.Property(e => e.FollowingId).HasColumnName("following_id");
            entity.Property(e => e.IsApproved)
                .HasDefaultValue(true)
                .HasColumnName("is_approved");

            entity.HasOne(d => d.Follower).WithMany(p => p.FollowFollowers)
                .HasForeignKey(d => d.FollowerId)
                .HasConstraintName("follows_follower_id_fkey");

            entity.HasOne(d => d.Following).WithMany(p => p.FollowFollowings)
                .HasForeignKey(d => d.FollowingId)
                .HasConstraintName("follows_following_id_fkey");
        });

        modelBuilder.Entity<Hashtag>(entity =>
        {
            entity.HasKey(e => e.HashtagId).HasName("hashtags_pkey");

            entity.ToTable("hashtags");

            entity.HasIndex(e => e.Name, "hashtags_name_key").IsUnique();

            entity.Property(e => e.HashtagId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("hashtag_id");
            entity.Property(e => e.FirstUsed)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("first_used");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UsageCount)
                .HasDefaultValue(0)
                .HasColumnName("usage_count");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.LikeId).HasName("likes_pkey");

            entity.ToTable("likes");

            entity.HasIndex(e => new { e.ContentType, e.ContentId }, "idx_likes_content");

            entity.HasIndex(e => e.UserId, "idx_likes_user_id");

            entity.HasIndex(e => new { e.UserId, e.ContentId, e.ContentType },
                "likes_user_id_content_id_content_type_key").IsUnique();

            entity.Property(e => e.LikeId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("like_id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.ContentType)
                .HasMaxLength(10)
                .HasColumnName("content_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Medium>(entity =>
        {
            entity.HasKey(e => e.MediaId).HasName("media_pkey");

            entity.ToTable("media");

            entity.Property(e => e.MediaId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("media_id");
            entity.Property(e => e.MediaType)
                .HasMaxLength(50)
                .HasColumnName("media_type");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(255)
                .HasColumnName("media_url");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(255)
                .HasColumnName("thumbnail_url");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Post).WithMany(p => p.Media)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("media_post_id_fkey");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("messages_pkey");

            entity.ToTable("messages");

            entity.HasIndex(e => new { e.ChatId, e.SentAt }, "idx_messages_chat_id_sent_at").IsDescending(false, true);

            entity.HasIndex(e => e.UserId, "idx_messages_user_id");

            entity.Property(e => e.MessageId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("message_id");
            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("sent_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Chat).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChatId)
                .HasConstraintName("messages_chat_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Messages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("messages_user_id_fkey");
        });

        modelBuilder.Entity<MessageMedium>(entity =>
        {
            entity.HasKey(e => e.MessageMediaId).HasName("message_media_pkey");

            entity.ToTable("message_media");

            entity.HasIndex(e => e.MessageId, "idx_message_media_message_id");

            entity.Property(e => e.MessageMediaId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("message_media_id");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.MediaType)
                .HasMaxLength(50)
                .HasColumnName("media_type");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(255)
                .HasColumnName("media_url");
            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Message).WithMany(p => p.MessageMedia)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("message_media_message_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("notifications_pkey");

            entity.ToTable("notifications");

            entity.HasIndex(e => e.CreatedAt, "idx_notifications_created_at").IsDescending();

            entity.HasIndex(e => e.UserId, "idx_notifications_user_id");

            entity.Property(e => e.NotificationId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("notification_id");
            entity.Property(e => e.ActorUserId).HasColumnName("actor_user_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .HasColumnName("notification_type");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(20)
                .HasColumnName("reference_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ActorUser).WithMany(p => p.NotificationActorUsers)
                .HasForeignKey(d => d.ActorUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notifications_actor_user_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("notifications_user_id_fkey");
        });

        modelBuilder.Entity<Post.Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("posts_pkey");

            entity.ToTable("posts");

            entity.HasIndex(e => e.CreatedAt, "idx_posts_created_at").IsDescending();

            entity.HasIndex(e => e.UserId, "idx_posts_user_id");

            entity.Property(e => e.PostId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("post_id");
            entity.Property(e => e.CommentCount)
                .HasDefaultValue(0)
                .HasColumnName("comment_count");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.PrivacyLevel)
                .HasDefaultValue((short)0)
                .HasColumnName("privacy_level");
            entity.Property(e => e.ShareCount)
                .HasDefaultValue(0)
                .HasColumnName("share_count");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("posts_user_id_fkey");

            entity.HasMany(d => d.Hashtags).WithMany(p => p.Posts)
                .UsingEntity<Dictionary<string, object>>(
                    "PostHashtag",
                    r => r.HasOne<Hashtag>().WithMany()
                        .HasForeignKey("HashtagId")
                        .HasConstraintName("post_hashtags_hashtag_id_fkey"),
                    l => l.HasOne<Post.Post>().WithMany()
                        .HasForeignKey("PostId")
                        .HasConstraintName("post_hashtags_post_id_fkey"),
                    j =>
                    {
                        j.HasKey("PostId", "HashtagId").HasName("post_hashtags_pkey");
                        j.ToTable("post_hashtags");
                        j.HasIndex(new[] { "HashtagId" }, "idx_post_hashtags_hashtag_id");
                        j.IndexerProperty<Guid>("PostId").HasColumnName("post_id");
                        j.IndexerProperty<Guid>("HashtagId").HasColumnName("hashtag_id");
                    });
        });

        modelBuilder.Entity<Reaction.Reaction>(entity =>
        {
            entity.HasKey(e => e.ReactionId).HasName("reactions_pkey");

            entity.ToTable("reactions");

            entity.HasDiscriminator(e => e.ContentType)
                .HasValue<PostReaction>("post")
                .HasValue<CommentReaction>("comment");

            entity.HasIndex(e => new { e.ContentType, e.ContentId }, "idx_reactions_content");

            entity.HasIndex(e => e.UserId, "idx_reactions_user_id");

            entity.HasIndex(e => new { e.UserId, e.ContentId, e.ContentType },
                "reactions_user_id_content_id_content_type_key").IsUnique();

            entity.Property(e => e.ReactionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("reaction_id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.ContentType)
                .HasMaxLength(10)
                .HasColumnName("content_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Reactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("reactions_user_id_fkey");

            entity.Property(d => d.ReactionType)
                .HasColumnType("smallint")
                .HasColumnName("reaction_type");
        });

        modelBuilder.Entity<CommentReaction>(p =>
        {
            MapBaseReaction(p);
            p.Property(d => d.ContentType)
                .HasMaxLength(10)
                .HasDefaultValue("comment");

            p.HasOne(p => p.Comment).WithMany(p => p.Reactions)
                .HasForeignKey(p => p.ContentId);
        });
        modelBuilder.Entity<PostReaction>(p =>
        {
            MapBaseReaction(p);
            p.Property(d => d.ContentType)
                .HasMaxLength(10)
                .HasDefaultValue("post");
            p.HasOne(p => p.Post).WithMany(p => p.Reactions)
                .HasForeignKey(p => p.ContentId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "idx_users_email");

            entity.HasIndex(e => e.Username, "idx_users_username");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("user_id");
            entity
                .HasIndex(e => e.OauthSub, "oauth_sub")
                .IsUnique();
            entity.Property(e => e.OauthSub)
                .HasColumnName("oauth_sub");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastLogin).HasColumnName("last_login");
            entity.Property(e => e.ProfilePicture)
                .HasMaxLength(255)
                .HasColumnName("profile_picture");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
            entity.Property(e => e.VerificationStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'unverified'::character varying")
                .HasColumnName("verification_status");
        });

        modelBuilder.Entity<UserSetting>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("user_settings_pkey");

            entity.ToTable("user_settings");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.LanguagePreference)
                .HasMaxLength(10)
                .HasDefaultValueSql("'en'::character varying")
                .HasColumnName("language_preference");
            entity.Property(e => e.NotificationsEnabled)
                .HasDefaultValue(true)
                .HasColumnName("notifications_enabled");
            entity.Property(e => e.PrivateAccount)
                .HasDefaultValue(false)
                .HasColumnName("private_account");
            entity.Property(e => e.ThemePreference)
                .HasMaxLength(20)
                .HasDefaultValueSql("'system'::character varying")
                .HasColumnName("theme_preference");

            entity.HasOne(d => d.User).WithOne(p => p.UserSetting)
                .HasForeignKey<UserSetting>(d => d.UserId)
                .HasConstraintName("user_settings_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    private static void MapBaseReaction<T>(EntityTypeBuilder<T> entity)
        where T : Reaction.Reaction, new()
    {
        entity.ToTable("reactions");

        entity.HasIndex(e => new { e.ContentType, e.ContentId }, "idx_reactions_content");

        entity.HasIndex(e => e.UserId, "idx_reactions_user_id");

        entity.HasIndex(e => new { e.UserId, e.ContentId, e.ContentType },
            "reactions_user_id_content_id_content_type_key").IsUnique();

        entity.Property(e => e.ReactionId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("reaction_id");
        entity.Property(e => e.ContentId).HasColumnName("content_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        entity.Property(e => e.UserId).HasColumnName("user_id");

        entity.Property(d => d.ReactionType)
            .HasColumnType("smallint")
            .HasColumnName("reaction_type");

        entity.Property(e => e.ContentType)
            .HasMaxLength(10)
            .HasColumnName("content_type");
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}