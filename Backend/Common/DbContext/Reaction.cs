using System.ComponentModel.DataAnnotations.Schema;
using Backend.Common.Helpers.Types;

namespace Backend.Common.DbContext;

public partial class Reaction
{
    public Guid ReactionId { get; set; }

    public Guid UserId { get; set; }

    public Guid ContentId { get; set; }

    public virtual string ContentType { get; set; } = string.Empty;

    public ReactionType ReactionType { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}