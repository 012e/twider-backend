using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(e => e.UserId).HasName("users_pkey");

        entity.ToTable("users");

        entity.HasIndex(e => e.Email, "idx_users_email");

        entity.HasIndex(e => e.Username, "idx_users_username");

        entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

        entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

        entity.Property(e => e.UserId)
            .HasColumnName("user_id");
        entity
            .HasIndex(e => e.OauthSub, "oauth_sub")
            .IsUnique();
        entity.Property(e => e.OauthSub)
            .HasColumnName("oauth_sub");
        entity.Property(e => e.Bio).HasColumnName("bio");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        entity.Property(e => e.Email)
            .HasMaxLength(255)
            .HasColumnName("email");
        entity.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");
        entity.Property(e => e.LastLogin).HasColumnName("last_login");
        entity.Property(e => e.ProfilePicture)
            .HasMaxLength(255)
            .HasColumnName("profile_picture");
        entity.Property(e => e.Username)
            .HasMaxLength(50)
            .HasColumnName("username");
        entity.Property(e => e.VerificationStatus)
            .HasMaxLength(20)
            .HasDefaultValueSql("'unverified'::character varying")
            .HasColumnName("verification_status");
    }
}