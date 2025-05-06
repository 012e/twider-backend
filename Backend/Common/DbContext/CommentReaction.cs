namespace Backend.Common.DbContext;

public class CommentReaction : Reaction
{
    public virtual Comment Comment { get; set; } = null!;
}