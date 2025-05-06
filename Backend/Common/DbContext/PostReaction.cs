namespace Backend.Common.DbContext;

public class PostReaction : Reaction
{
    public virtual Post Post { get; set; } = null!;
}