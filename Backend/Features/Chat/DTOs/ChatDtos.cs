namespace Backend.Features.Chat.DTOs;

public class ChatDto
{
    public Guid ChatId { get; set; }
    public string? ChatName { get; set; }
    public string ChatType { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int MessageCount { get; set; }
    public List<ChatParticipantDto> Participants { get; set; } = new();
    public MessageDto? LastMessage { get; set; }
}

public class ChatParticipantDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string? ProfilePicture { get; set; }
    public DateTime JoinedAt { get; set; }
    public string Role { get; set; } = null!;
    public Guid? LastReadMessageId { get; set; }
}

public class MessageDto
{
    public Guid MessageId { get; set; }
    public Guid ChatId { get; set; }
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Content { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsDeleted { get; set; }
}
