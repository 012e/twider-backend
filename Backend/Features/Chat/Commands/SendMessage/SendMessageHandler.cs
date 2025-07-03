using Backend.Common.DbContext;
using Backend.Common.DbContext.Chat;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Chat.Commands.SendMessage;

public class SendMessageHandler : IRequestHandler<SendMessageCommand, ApiResult<ItemId>>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public SendMessageHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<ItemId>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var user = _currentUserService.User;
        if (user == null)
        {
            return ApiResult<ItemId>.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "User not found."
            });
        }

        // Can't send message to yourself
        if (user.UserId == request.OtherUserId)
        {
            return ApiResult<ItemId>.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid request",
                Detail = "Cannot send message to yourself."
            });
        }

        // Validate that the other user exists
        var otherUserExists = await _db.Users
            .AnyAsync(u => u.UserId == request.OtherUserId && u.IsActive, cancellationToken);

        if (!otherUserExists)
        {
            return ApiResult<ItemId>.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "User not found",
                Detail = $"User with ID {request.OtherUserId} not found or inactive."
            });
        }

        // Find or create the direct message chat between current user and other user
        var directChat = await _db.Chats
            .Where(c => c.ChatType == "direct" && 
                       c.ChatParticipants.Count == 2 &&
                       c.ChatParticipants.Any(cp => cp.UserId == user.UserId) &&
                       c.ChatParticipants.Any(cp => cp.UserId == request.OtherUserId))
            .FirstOrDefaultAsync(cancellationToken);

        if (directChat == null)
        {
            // Create a new direct message chat
            directChat = new Common.DbContext.Chat.Chat
            {
                ChatId = Guid.NewGuid(),
                ChatName = null, // Direct messages don't have names
                ChatType = "direct",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                MessageCount = 0
            };

            _db.Chats.Add(directChat);

            // Add participants
            var currentUserParticipant = new ChatParticipant
            {
                ChatId = directChat.ChatId,
                UserId = user.UserId,
                JoinedAt = DateTime.UtcNow,
                Role = "member"
            };

            var otherUserParticipant = new ChatParticipant
            {
                ChatId = directChat.ChatId,
                UserId = request.OtherUserId,
                JoinedAt = DateTime.UtcNow,
                Role = "member"
            };

            _db.ChatParticipants.Add(currentUserParticipant);
            _db.ChatParticipants.Add(otherUserParticipant);
        }

        // Create the message
        var message = new Message
        {
            MessageId = Guid.NewGuid(),
            ChatId = directChat.ChatId,
            UserId = user.UserId,
            Content = request.Content,
            SentAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _db.Messages.Add(message);

        // Update chat's updated_at and message count
        directChat.UpdatedAt = DateTime.UtcNow;
        directChat.MessageCount++;

        await _db.SaveChangesAsync(cancellationToken);

        return ApiResult<ItemId>.Ok(new ItemId(message.MessageId));
    }
}
