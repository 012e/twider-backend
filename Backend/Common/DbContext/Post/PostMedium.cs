namespace Backend.Common.DbContext.Post;

public class PostMedium : Medium
{
    public virtual Post Post { get; set; } = null!;
    public Guid PostId { get; set; }
}