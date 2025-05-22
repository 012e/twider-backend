using Backend.Common.DbContext;
using Backend.Common.DbContext.Reaction;
using Backend.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Post.Commands.AddReaction;

public class PostReactionHandler : IRequestHandler<PostReactionCommand, ApiResult<Unit>>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public PostReactionHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<Unit>> Handle(PostReactionCommand request, CancellationToken cancellationToken)
    {
        var post = await _db.Posts
            .FirstOrDefaultAsync(i => i.PostId == request.PostId, cancellationToken: cancellationToken);
        if (post is null)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Title = "Post not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Post with ID {request.PostId} not found."
            });
        }

        var reactionType = request.ReactionType.ReactionType;

        var user = _currentUserService.User;
        if (user is null)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "You must be logged in to react to posts."
            });
        }

        // Check if the user has already reacted to this post
        var existingReaction = await _db.PostReactions
            .FirstOrDefaultAsync(r =>
                    r.UserId == user.UserId &&
                    r.ContentId == request.PostId,
                cancellationToken);

        if (existingReaction != null)
        {
            // Update existing reaction if the type is different
            if (existingReaction.ReactionType != reactionType)
            {
                existingReaction.ReactionType = reactionType;
                await _db.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }

        // Create a new reaction
        var reaction = new PostReaction
        {
            UserId = user.UserId,
            ContentId = request.PostId,
            ReactionType = reactionType,
            ContentType = "post",
            CreatedAt = DateTime.UtcNow,
        };

        await _db.PostReactions.AddAsync(reaction, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}