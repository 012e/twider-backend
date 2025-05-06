using Backend.Common.DbContext;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Comment.Commands;

public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, ApiResult<ItemId>>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public CreateCommentHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<ItemId>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var user = _currentUserService.User;
        var content = request.Content;

        if (user == null)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "User must be logged in to comment",
            });
        }

        // validate content
        if (string.IsNullOrWhiteSpace(content.Content))
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Title = "Content cannot be empty",
                Detail = "Comment content cannot be empty.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // validate parent comment
        if (request.ParentCommentId is not null)
        {
            var parent = await _db.Comments
                .FirstOrDefaultAsync(c => c.CommentId == request.ParentCommentId, cancellationToken);
            if (parent is null)
            {
                return ApiResult.Fail(new ProblemDetails
                {
                    Title = "Parent comment not found",
                    Detail = $"Parent comment with ID {request.ParentCommentId} not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }
        }


        // validate post
        var post = await _db.Posts
            .FirstOrDefaultAsync(p => p.PostId == request.PostId, cancellationToken);
        if (post is null)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Title = "Post not found",
                Detail = $"Post with ID {request.PostId} not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var comment = new Common.DbContext.Comment
        {
            PostId = request.PostId,
            Content = content.Content,
            UserId = user.UserId,
            ParentCommentId = request.ParentCommentId
        };

        //  TODO: Update CommentCount in Post
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync(cancellationToken);

        return ApiResult.Ok(new ItemId(comment.CommentId));
    }
}