namespace Backend.Features.User.Queries;

public class GetUserByIdResponse
{
    public Guid UserId { get; set; }

    public string OauthSub { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string? Email { get; set; } = null!;

    public string? ProfilePicture { get; set; }

    public string? Bio { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool IsActive { get; set; }

    public string VerificationStatus { get; set; } = null!;
}