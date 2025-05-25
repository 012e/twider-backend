using Backend.Common.DbContext;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Comment.Commands.DeleteReaction;

public class DeleteReactionHandler : IRequestHandler<DeleteReactionCommand, ApiResult<Unit>>
{
    private readonly ApplicationDbContext _db;

    public DeleteReactionHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResult<Unit>> Handle(DeleteReactionCommand request, CancellationToken cancellationToken)
    {
        var commentExists = await _db.Comments.AnyAsync(
            i => i.CommentId == request.CommentId && i.PostId == request.PostId,
            cancellationToken: cancellationToken);

        if (!commentExists)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Title = "Comment do not exists",
                Status = 404,
                Detail = $"Comment with id {request.CommentId} do not exists"
            });
        }

        _db.CommentReactions.RemoveRange(
            _db.CommentReactions.Where(i => i.ContentId == request.CommentId)
        );
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}