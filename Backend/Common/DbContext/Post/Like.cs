namespace Backend.Common.DbContext.Post;

public partial class Like
{
    public Guid LikeId { get; set; }

    public Guid UserId { get; set; }

    public Guid ContentId { get; set; }

    public string ContentType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
