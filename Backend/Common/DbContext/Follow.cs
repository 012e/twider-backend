namespace Backend.Common.DbContext;

public partial class Follow
{
    public int FollowId { get; set; }

    public int FollowerId { get; set; }

    public int FollowingId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsApproved { get; set; }

    public virtual User Follower { get; set; } = null!;

    public virtual User Following { get; set; } = null!;
}
