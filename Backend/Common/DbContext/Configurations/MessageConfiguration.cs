using Backend.Common.DbContext.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> entity)
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
    }
}