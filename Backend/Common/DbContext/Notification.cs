namespace Backend.Common.DbContext;

public partial class Notification
{
    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    public Guid? ActorUserId { get; set; }

    public string NotificationType { get; set; } = null!;

    public string? Content { get; set; }

    public Guid? ReferenceId { get; set; }

    public string? ReferenceType { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? ActorUser { get; set; }

    public virtual User User { get; set; } = null!;
}
