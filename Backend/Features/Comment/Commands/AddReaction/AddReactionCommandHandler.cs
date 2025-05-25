using Backend.Common.DbContext;
using Backend.Common.DbContext.Reaction;
using Backend.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Comment.Commands.AddReaction;

public class AddReactionCommandHandler : IRequestHandler<AddReactionCommand, ApiResult<Unit>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public AddReactionCommandHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<Unit>> Handle(AddReactionCommand request, CancellationToken cancellationToken)
    {
        var comment = await _db.Comments
            .FirstOrDefaultAsync(i => i.CommentId == request.CommentId && i.PostId == request.PostId,
                cancellationToken: cancellationToken);

        if (comment is null)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Title = "Comment not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Comment with ID {request.CommentId} not found."
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
        var existingReaction = await _db.CommentReactions
            .FirstOrDefaultAsync(r =>
                    r.UserId == user.UserId &&
                    r.ContentId == request.CommentId,
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
        var reaction = new CommentReaction
        {
            UserId = user.UserId,
            ContentId = request.CommentId,
            ReactionType = reactionType,
            ContentType = "comment",
            CreatedAt = DateTime.UtcNow,
        };

        await _db.CommentReactions.AddAsync(reaction, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}