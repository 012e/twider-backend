namespace Backend.Common.DbContext;

public partial class Message
{
    public long MessageId { get; set; }

    public int ChatId { get; set; }

    public int? UserId { get; set; }

    public string? Content { get; set; }

    public DateTime SentAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual ICollection<MessageMedium> MessageMedia { get; set; } = new List<MessageMedium>();

    public virtual User? User { get; set; }
}
