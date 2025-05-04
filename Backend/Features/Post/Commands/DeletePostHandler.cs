using Backend.Common.DbContext;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Post.Commands;

public class DeletePostHandler : IRequestHandler<DeletePostCommand, ApiResult<Unit>>
{
    private readonly ApplicationDbContext _db;

    public DeletePostHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResult<Unit>> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(i => i.PostId == request.Id, cancellationToken: cancellationToken);
        if (post is null)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Title = "Post does not found",
                Detail = $"Post with ID {request.Id} does not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResult.Ok(Unit.Value);
    }
}