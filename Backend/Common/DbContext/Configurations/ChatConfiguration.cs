using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<Chat.Chat>
{
    public void Configure(EntityTypeBuilder<Chat.Chat> entity)
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
    }
}