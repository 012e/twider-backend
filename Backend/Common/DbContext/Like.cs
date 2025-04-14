namespace Backend.Common.DbContext;

public partial class Like
{
    public int LikeId { get; set; }

    public int UserId { get; set; }

    public int ContentId { get; set; }

    public string ContentType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
