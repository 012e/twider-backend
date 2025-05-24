using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> entity)
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
    }
}