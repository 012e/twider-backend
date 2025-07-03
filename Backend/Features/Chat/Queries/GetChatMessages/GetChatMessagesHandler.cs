using Backend.Common.DbContext;
using Backend.Common.Helpers;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using Backend.Features.Chat.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Chat.Queries.GetChatMessages;

public class GetChatMessagesHandler : IRequestHandler<GetChatMessagesQuery, ApiResult<InfiniteCursorPage<MessageDto>>>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public GetChatMessagesHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<InfiniteCursorPage<MessageDto>>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var user = _currentUserService.User;
        if (user == null)
        {
            return ApiResult<InfiniteCursorPage<MessageDto>>.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "User not found."
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
            // Return empty result if chat doesn't exist
            return ApiResult<InfiniteCursorPage<MessageDto>>.Ok(new InfiniteCursorPage<MessageDto>(
                items: new List<MessageDto>(),
                nextCursor: null,
                hasMore: false
            ));
        }

        // Build the messages query
        var messagesQuery = _db.Messages
            .Where(m => m.ChatId == directChat.ChatId && !m.IsDeleted)
            .Select(m => new MessageDto
            {
                MessageId = m.MessageId,
                ChatId = m.ChatId,
                UserId = m.UserId,
                Username = m.User != null ? m.User.Username : null,
                ProfilePicture = m.User != null ? m.User.ProfilePicture : null,
                Content = m.Content,
                SentAt = m.SentAt,
                IsDeleted = m.IsDeleted
            });

        // Apply cursor filters
        if (!string.IsNullOrEmpty(request.Before))
        {
            try
            {
                var beforeMessageId = Guid.Parse(CursorEncoder.Decode(request.Before));
                var beforeMessage = await _db.Messages
                    .Where(m => m.MessageId == beforeMessageId)
                    .Select(m => new { m.SentAt, m.MessageId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (beforeMessage != null)
                {
                    messagesQuery = messagesQuery.Where(m => 
                        m.SentAt < beforeMessage.SentAt || 
                        (m.SentAt == beforeMessage.SentAt && m.MessageId.CompareTo(beforeMessage.MessageId) < 0));
                }
            }
            catch (Exception)
            {
                // Invalid cursor format, ignore
            }
        }

        if (!string.IsNullOrEmpty(request.After))
        {
            try
            {
                var afterMessageId = Guid.Parse(CursorEncoder.Decode(request.After));
                var afterMessage = await _db.Messages
                    .Where(m => m.MessageId == afterMessageId)
                    .Select(m => new { m.SentAt, m.MessageId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (afterMessage != null)
                {
                    messagesQuery = messagesQuery.Where(m => 
                        m.SentAt > afterMessage.SentAt || 
                        (m.SentAt == afterMessage.SentAt && m.MessageId.CompareTo(afterMessage.MessageId) > 0));
                }
            }
            catch (Exception)
            {
                // Invalid cursor format, ignore
            }
        }

        // Order by sent time (most recent first)
        messagesQuery = messagesQuery.OrderByDescending(m => m.SentAt).ThenByDescending(m => m.MessageId);

        // Take one extra item to determine if there are more pages
        var messages = await messagesQuery.Take(request.PageSize + 1).ToListAsync(cancellationToken);

        // Determine if there are more items and prepare the result
        var hasMore = messages.Count > request.PageSize;
        var itemsToReturn = hasMore ? messages.Take(request.PageSize).ToList() : messages;

        // Generate next cursor from the last item
        var nextCursor = itemsToReturn.Any() 
            ? CursorEncoder.Encode(itemsToReturn.Last().MessageId.ToString()) 
            : null;

        var result = new InfiniteCursorPage<MessageDto>(
            items: itemsToReturn,
            nextCursor: nextCursor,
            hasMore: hasMore
        );

        return ApiResult<InfiniteCursorPage<MessageDto>>.Ok(result);
    }
}
