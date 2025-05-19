using Backend.Common.DbContext;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Comment.Commands;

public class DeleteCommandHandler : IRequestHandler<DeleteCommentCommand, ApiResult<Unit>>
{
    private readonly ApplicationDbContext _db;

    public DeleteCommandHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResult<Unit>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var postExists =
            await _db.Posts.AnyAsync(i => i.PostId == request.PostId, cancellationToken: cancellationToken);
        if (!postExists)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Post not found",
                Detail = $"Post with ID {request.PostId} not found."
            });
        }

        var comment = await _db.Comments
            .FirstOrDefaultAsync(
                c => c.CommentId == request.CommentId && c.PostId == request.PostId,
                cancellationToken: cancellationToken);

        if (comment is null)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Comment not found",
                Detail = $"Comment with ID {request.CommentId} not found."
            });
        }

        _db.Comments.Remove(comment);

        await _db.SaveChangesAsync(cancellationToken);
        return ApiResult.Ok(Unit.Value);
    }
}