using Backend.Common.DbContext.Post;
using Backend.Common.DbContext.Reaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> entity)
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
    }
}