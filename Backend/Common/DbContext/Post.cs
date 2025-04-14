namespace Backend.Common.DbContext;

public partial class Post
{
    public int PostId { get; set; }

    public int UserId { get; set; }

    public string? Content { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public short PrivacyLevel { get; set; }

    public int LikeCount { get; set; }

    public int CommentCount { get; set; }

    public int ShareCount { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Medium> Media { get; set; } = new List<Medium>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Hashtag> Hashtags { get; set; } = new List<Hashtag>();
}
