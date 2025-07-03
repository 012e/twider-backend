using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Types;
using Backend.Features.Chat.DTOs;
using MediatR;

namespace Backend.Features.Chat.Queries.GetChatMessages;

public class GetChatMessagesQuery : IRequest<ApiResult<InfiniteCursorPage<MessageDto>>>
{
    [Required]
    public Guid OtherUserId { get; set; }

    public string? Before { get; set; }
    public string? After { get; set; }

    [Range(1, 100)]
    public int PageSize { get; set; } = 50;
}
