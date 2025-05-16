namespace Backend.Common.DbContext.Chat;

public partial class Chat
{
    public Guid ChatId { get; set; }

    public string? ChatName { get; set; }

    public string ChatType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int MessageCount { get; set; }

    public virtual ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
