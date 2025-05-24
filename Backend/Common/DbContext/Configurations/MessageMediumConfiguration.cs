using Backend.Common.DbContext.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class MessageMediumConfiguration : IEntityTypeConfiguration<MessageMedium>
{
    public void Configure(EntityTypeBuilder<MessageMedium> entity)
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
    }
}