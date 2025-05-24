using Backend.Common.DbContext.Post;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Common.DbContext.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> entity)
    {
        entity.HasKey(e => e.CommentId).HasName("comments_pkey");

        entity.ToTable("comments");

        entity.HasIndex(e => e.ParentCommentId, "idx_comments_parent_comment_id");

        entity.HasIndex(e => e.PostId, "idx_comments_post_id");

        entity.HasIndex(e => e.UserId, "idx_comments_user_id");

        entity.Property(e => e.CommentId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("comment_id");
        entity.Property(e => e.Content).HasColumnName("content");
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .HasColumnName("created_at");
        entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");
        entity.Property(e => e.PostId).HasColumnName("post_id");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        entity.Property(e => e.UserId).HasColumnName("user_id");

        entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment)
            .HasForeignKey(d => d.ParentCommentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("comments_parent_comment_id_fkey");

        entity.HasOne(d => d.Post).WithMany(p => p.Comments)
            .HasForeignKey(d => d.PostId)
            .HasConstraintName("comments_post_id_fkey");

        entity.HasOne(d => d.User).WithMany(p => p.Comments)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("comments_user_id_fkey");
    }
}