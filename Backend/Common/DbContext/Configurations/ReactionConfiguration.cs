using Backend.Common.DbContext.Post;
using Backend.Common.DbContext.Reaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class ReactionConfiguration : IEntityTypeConfiguration<Reaction.Reaction>
{
    public void Configure(EntityTypeBuilder<Reaction.Reaction> entity)
    {
        entity.ToTable("reactions");
        entity.HasKey(e => e.ReactionId).HasName("reactions_pkey");

        entity.HasIndex(e => new { e.ContentType, e.ContentId }, "idx_reactions_content");
        entity.HasIndex(e => e.UserId, "idx_reactions_user_id");
        entity.HasIndex(e => new { e.UserId, e.ContentId, e.ContentType },
            "reactions_user_id_content_id_content_type_key").IsUnique();

        entity.Property(e => e.ReactionId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("reaction_id");
        entity.Property(e => e.ContentId).HasColumnName("content_id");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(d => d.ReactionType)
            .HasColumnName("reaction_type")
            .HasColumnType("smallint")
            .HasMaxLength(10);

        entity.HasDiscriminator(e => e.ContentType)
            .HasValue<PostReaction>("post")
            .HasValue<CommentReaction>("comment");

        entity.Property(e => e.ContentType)
            .HasColumnType("varchar(50)")
            .HasColumnName("content_type");
    }
}

public class PostReactionConfiguration : IEntityTypeConfiguration<PostReaction>
{
    public void Configure(EntityTypeBuilder<PostReaction> entity)
    {
        entity.HasOne(a => a.Post).WithMany(a => a.Reactions)
            .HasForeignKey(a => a.ContentId);

        entity.Property(e => e.ContentType)
            .HasColumnType("varchar(50)")
            .HasColumnName("content_type");
    }
}

public class CommentReactionConfiguration : IEntityTypeConfiguration<CommentReaction>
{
    public void Configure(EntityTypeBuilder<CommentReaction> entity)
    {
        entity.HasOne(a => a.Comment).WithMany(a => a.Reactions)
            .HasForeignKey(a => a.ContentId);

        entity.Property(e => e.ContentType)
            .HasColumnType("varchar(50)")
            .HasColumnName("content_type");
    }
}