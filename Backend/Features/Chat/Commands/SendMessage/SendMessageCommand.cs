using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Types;
using MediatR;

namespace Backend.Features.Chat.Commands.SendMessage;

public class SendMessageCommand : IRequest<ApiResult<ItemId>>
{
    [Required]
    public Guid OtherUserId { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Content { get; set; } = null!;
}
