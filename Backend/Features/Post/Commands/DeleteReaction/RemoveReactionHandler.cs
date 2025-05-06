using Backend.Common.DbContext;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Post.Commands.DeleteReaction;

public class RemoveReactionHandler : IRequestHandler<RemoveReactionCommand, ApiResult<Unit>>
{
    private readonly ApplicationDbContext _db;

    public RemoveReactionHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResult<Unit>> Handle(RemoveReactionCommand request, CancellationToken cancellationToken)
    {
        var postExists =
            await _db.Posts.AnyAsync(i => i.PostId == request.PostId, cancellationToken: cancellationToken);

        if (!postExists)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Title = "Post do not exists",
                Status = 404,
                Detail = $"Post with id {request.PostId} do not exists"
            });
        }

        _db.PostReactions.RemoveRange(
            _db.PostReactions.Where(i => i.ContentId == request.PostId)
        );
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}