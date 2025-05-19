using MediatR;

namespace Backend.Features.Comment.Commands;

public class DeleteCommentCommand : IRequest<ApiResult<Unit>>
{
    public Guid CommentId { get; set; }
    public Guid PostId { get; set; }
}