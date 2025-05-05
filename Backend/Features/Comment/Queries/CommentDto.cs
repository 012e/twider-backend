namespace Backend.Features.Comment.Queries;

public class CommentDto
{
    public Guid CommentId { get; set; }
    public string Content { get; set; } = null!;
    public UserDto User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Guid? ParentCommentId { get; set; }

    public int TotalReplies { get; set; }

    public class UserDto
    {
        public Guid UserId { get; set; }

        public string OauthSub { get; set; }

        public string Username { get; set; } = null!;

        public string? Email { get; set; } = null!;

        public string? ProfilePicture { get; set; }

        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLogin { get; set; }

        public bool IsActive { get; set; }

        public string VerificationStatus { get; set; } = null!;
    }
}