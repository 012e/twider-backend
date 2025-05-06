using MediatR;

namespace Backend.Features.Post.Commands.DeleteReaction;

public class RemoveReactionCommand : IRequest<ApiResult<Unit>>
{
    public required Guid PostId { get; set; }
}