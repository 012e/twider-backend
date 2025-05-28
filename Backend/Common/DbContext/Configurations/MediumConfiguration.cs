using Backend.Common.DbContext.Post;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class MediumConfiguration : IEntityTypeConfiguration<Medium>
{
    public void Configure(EntityTypeBuilder<Medium> entity)
    {
        entity.HasKey(e => e.MediaId).HasName("media_pkey");

        entity.ToTable("media");
        entity.Property(e => e.MediaId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("media_id");
        entity.Property(e => e.OwnerType)
            .HasMaxLength(50)
            .HasColumnName("media_owner_type");
        entity.Property(e => e.Type)
            .HasMaxLength(50)
            .HasColumnName("media_type");
        entity.Property(e => e.Path)
            .HasMaxLength(255)
            .HasColumnName("media_path");
        entity.Property(e => e.Url)
            .HasMaxLength(255)
            .HasColumnName("media_url");
        entity.Property(e => e.ThumbnailUrl)
            .HasMaxLength(255)
            .HasColumnName("thumbnail_url");
        entity.Property(e => e.UploadedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("uploaded_at");

        entity.HasDiscriminator(e => e.OwnerType)
            .HasValue<PostMedium>("post")
            .HasValue<CommentMedium>("comment")
            .HasValue<UnknownMedium>("unknown");
    }
}

public class PostMediumConfiguration : IEntityTypeConfiguration<PostMedium>
{
    public void Configure(EntityTypeBuilder<PostMedium> entity)
    {
        entity.Property(e => e.OwnerType)
            .HasMaxLength(50)
            .HasColumnName("media_owner_type")
            .HasDefaultValue("post");

        entity.Property(e => e.PostId)
            .HasColumnName("parent_id");

        entity.HasOne(d => d.Post).WithMany(p => p.Media)
            .HasForeignKey(d => d.PostId);
    }
}

public class CommentMediumConfiguration : IEntityTypeConfiguration<CommentMedium>
{
    public void Configure(EntityTypeBuilder<CommentMedium> entity)
    {
        entity.Property(e => e.OwnerType)
            .HasMaxLength(50)
            .HasColumnName("media_owner_type")
            .HasDefaultValue("comment");
    }
}

public class UnknownMediumConfiguration : IEntityTypeConfiguration<UnknownMedium>
{
    public void Configure(EntityTypeBuilder<UnknownMedium> entity)
    {
        entity.Property(e => e.OwnerType)
            .HasMaxLength(50)
            .HasColumnName("media_owner_type")
            .HasDefaultValue("unknown");
    }
}