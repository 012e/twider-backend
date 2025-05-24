using Backend.Common.DbContext.Post;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class HashtagConfiguration : IEntityTypeConfiguration<Hashtag>
{
    public void Configure(EntityTypeBuilder<Hashtag> entity)
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
    }
}