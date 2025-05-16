namespace Backend.Common.DbContext.Post;

public partial class Hashtag
{
    public Guid HashtagId { get; set; }

    public string Name { get; set; } = null!;

    public int UsageCount { get; set; }

    public DateTime FirstUsed { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
