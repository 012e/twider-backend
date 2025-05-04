namespace Backend.Common.DbContext;

public partial class ChatParticipant
{
    public Guid ChatId { get; set; }

    public Guid UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public string Role { get; set; } = null!;

    public Guid? LastReadMessageId { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
