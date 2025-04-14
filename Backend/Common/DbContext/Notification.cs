namespace Backend.Common.DbContext;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public int? ActorUserId { get; set; }

    public string NotificationType { get; set; } = null!;

    public string? Content { get; set; }

    public int? ReferenceId { get; set; }

    public string? ReferenceType { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? ActorUser { get; set; }

    public virtual User User { get; set; } = null!;
}
