namespace Backend.Common.DbContext;

public partial class Message
{
    public Guid MessageId { get; set; }

    public Guid ChatId { get; set; }

    public Guid? UserId { get; set; }

    public string? Content { get; set; }

    public DateTime SentAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual ICollection<MessageMedium> MessageMedia { get; set; } = new List<MessageMedium>();

    public virtual User? User { get; set; }
}
