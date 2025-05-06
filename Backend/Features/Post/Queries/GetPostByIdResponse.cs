using Backend.Features.Post.Commands;
using Riok.Mapperly.Abstractions;

namespace Backend.Features.Post.Queries;

public class GetPostByIdResponse
{
    public required Guid PostId { get; set; }
    public required string Content { get; set; } = string.Empty;
    public required UserDto User { get; set; } = null!;
    public required DateTime CreatedAt { get; set; }

    public required DateTime? UpdatedAt { get; set; }

    public required ReactionDto Reactions { get; set; }

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

    public class ReactionDto
    {
        public required int Like { get; set; } = 0;
        public required int Love { get; set; } = 0;
        public required int Haha { get; set; } = 0;
        public required int Wow { get; set; } = 0;
        public required int Sad { get; set; } = 0;
        public required int Angry { get; set; } = 0;
        public required int Care { get; set; } = 0;
    }
}

[Mapper]
public static partial class UserMapper
{
    public static GetPostByIdResponse.UserDto ToUserDto(this Common.DbContext.User user)
    {
        return new GetPostByIdResponse.UserDto
        {
            UserId = user.UserId,
            OauthSub = user.OauthSub,
            Username = user.Username,
            Email = user.Email,
            ProfilePicture = user.ProfilePicture,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin,
            IsActive = user.IsActive,
            VerificationStatus = user.VerificationStatus
        };
    }
}