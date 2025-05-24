using Backend.Common.DbContext.Post;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post.Post>
{
    public void Configure(EntityTypeBuilder<Post.Post> entity)
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
    }
}