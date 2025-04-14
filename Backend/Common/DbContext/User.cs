namespace Backend.Common.DbContext;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? ProfilePicture { get; set; }

    public string? Bio { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool IsActive { get; set; }

    public string VerificationStatus { get; set; } = null!;

    public virtual ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Follow> FollowFollowers { get; set; } = new List<Follow>();

    public virtual ICollection<Follow> FollowFollowings { get; set; } = new List<Follow>();

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Notification> NotificationActorUsers { get; set; } = new List<Notification>();

    public virtual ICollection<Notification> NotificationUsers { get; set; } = new List<Notification>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual UserSetting? UserSetting { get; set; }
}
