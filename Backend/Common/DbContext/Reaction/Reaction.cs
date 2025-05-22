using Backend.Common.Helpers.Types;

namespace Backend.Common.DbContext.Reaction;

public class Reaction
{
    public Guid ReactionId { get; set; }

    public Guid UserId { get; set; }

    public Guid ContentId { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public ReactionType ReactionType { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}