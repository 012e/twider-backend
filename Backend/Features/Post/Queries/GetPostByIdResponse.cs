namespace Backend.Features.Post.Queries;

public class GetPostByIdResponse
{
    public Guid PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;

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