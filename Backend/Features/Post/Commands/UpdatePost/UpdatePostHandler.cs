using Backend.Common.DbContext;
using Backend.Features.Post.Commands.UpdatePost;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Post.Commands.AddReaction;

public class UpdatePostHandler : IRequestHandler<UpdatePostCommand, ApiResult<Unit>>
{
    private readonly ApplicationDbContext _db;

    public UpdatePostHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResult<Unit>> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(i => i.PostId == request.PostId, cancellationToken: cancellationToken);
        if (post is null)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Title = "Post not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Post with ID {request.PostId} not found."
            });
        }

        post.Content = request.Content.Content;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}