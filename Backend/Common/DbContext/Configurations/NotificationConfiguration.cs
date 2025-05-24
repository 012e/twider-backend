using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> entity)
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
    }
}