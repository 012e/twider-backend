namespace Backend.Common.DbContext;

public partial class ChatParticipant
{
    public int ChatId { get; set; }

    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public string Role { get; set; } = null!;

    public long? LastReadMessageId { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
