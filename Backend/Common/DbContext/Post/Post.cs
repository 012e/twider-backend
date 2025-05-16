using Backend.Common.DbContext.Reaction;

namespace Backend.Common.DbContext.Post;

public partial class Post
{
    public Guid PostId { get; set; }

    public Guid UserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public short PrivacyLevel { get; set; }

    public int CommentCount { get; set; }

    public int ShareCount { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Medium> Media { get; set; } = new List<Medium>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Hashtag> Hashtags { get; set; } = new List<Hashtag>();

    public virtual ICollection<PostReaction> Reactions { get; set; } = new List<PostReaction>();
}
