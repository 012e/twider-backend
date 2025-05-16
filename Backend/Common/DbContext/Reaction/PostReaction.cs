namespace Backend.Common.DbContext.Reaction;

public class PostReaction : Reaction
{
    public virtual Post.Post Post { get; set; } = null!;
}