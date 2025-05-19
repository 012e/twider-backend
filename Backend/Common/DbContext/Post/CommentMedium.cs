namespace Backend.Common.DbContext.Post;

public class CommentMedium : Medium
{
    public virtual Comment Comment { get; set; } = null!;
    public Guid CommentId { get; set; }
}