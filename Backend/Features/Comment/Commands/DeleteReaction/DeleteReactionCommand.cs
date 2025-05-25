using MediatR;

namespace Backend.Features.Comment.Commands.DeleteReaction;

public class DeleteReactionCommand : IRequest<ApiResult<Unit>>
{
    public required Guid CommentId { get; set; }
    public required Guid PostId { get; set; }
}