using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class UserSettingConfiguration : IEntityTypeConfiguration<UserSetting>
{
    public void Configure(EntityTypeBuilder<UserSetting> entity)
    {
        entity.HasKey(e => e.UserId).HasName("user_settings_pkey");

        entity.ToTable("user_settings");

        entity.Property(e => e.UserId)
            .ValueGeneratedNever()
            .HasColumnName("user_id");
        entity.Property(e => e.LanguagePreference)
            .HasMaxLength(10)
            .HasDefaultValueSql("'en'::character varying")
            .HasColumnName("language_preference");

        entity.HasOne(d => d.User).WithOne(p => p.UserSetting)
            .HasForeignKey<UserSetting>(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientCascade)
            .HasConstraintName("user_settings_user_id_fkey");
    }
}