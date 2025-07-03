using Backend.Common.DbContext;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using Backend.Features.Chat.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Chat.Queries.GetChats;

public class GetChatsHandler : IRequestHandler<GetChatsQuery, ApiResult<InfiniteCursorPage<ChatDto>>>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public GetChatsHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<InfiniteCursorPage<ChatDto>>> Handle(GetChatsQuery request, CancellationToken cancellationToken)
    {
        var user = _currentUserService.User;
        if (user == null)
        {
            return ApiResult<InfiniteCursorPage<ChatDto>>.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "User not found."
            });
        }

        // Get chats where current user is a participant
        var chatsQuery = _db.Chats
            .Where(c => c.ChatParticipants.Any(cp => cp.UserId == user.UserId))
            .Select(c => new ChatDto
            {
                ChatId = c.ChatId,
                ChatName = c.ChatName,
                ChatType = c.ChatType,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                MessageCount = c.MessageCount,
                Participants = c.ChatParticipants
                    .Select(cp => new ChatParticipantDto
                    {
                        UserId = cp.UserId,
                        Username = cp.User.Username,
                        ProfilePicture = cp.User.ProfilePicture,
                        JoinedAt = cp.JoinedAt,
                        Role = cp.Role,
                        LastReadMessageId = cp.LastReadMessageId
                    })
                    .ToList(),
                LastMessage = c.Messages
                    .Where(m => !m.IsDeleted)
                    .OrderByDescending(m => m.SentAt)
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
                    })
                    .FirstOrDefault()
            });

        // Use pagination service to get results
        var result = await InfinitePaginationService.PaginateAsync(
            chatsQuery,
            keySelector: c => c.ChatId,
            after: request.PaginationMeta.Cursor,
            limit: request.PaginationMeta.PageSize,
            ascending: false // Most recent chats first
        );

        return ApiResult<InfiniteCursorPage<ChatDto>>.Ok(result);
    }
}
