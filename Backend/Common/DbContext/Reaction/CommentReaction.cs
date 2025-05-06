namespace Backend.Common.DbContext.Reaction;

public class CommentReaction : Reaction
{
    public virtual Comment Comment { get; set; } = null!;
}