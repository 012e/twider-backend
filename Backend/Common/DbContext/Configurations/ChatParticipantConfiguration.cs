using Backend.Common.DbContext.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class ChatParticipantConfiguration : IEntityTypeConfiguration<ChatParticipant>
{
    public void Configure(EntityTypeBuilder<ChatParticipant> entity)
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
    }
}