using Backend.Common.DbContext;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using Backend.Features.Comment.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Comment.Commands.CreateComment;

public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, ApiResult<CommentDto>>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public CreateCommentHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<CommentDto>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
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

        var comment = new Common.DbContext.Post.Comment
        {
            PostId = request.PostId,
            Content = content.Content,
            UserId = user.UserId,
            ParentCommentId = request.ParentCommentId
        };

        //  TODO: Update CommentCount in Post
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync(cancellationToken);

        // Create CommentDto to return
        var commentDto = new CommentDto
        {
            CommentId = comment.CommentId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            ParentCommentId = comment.ParentCommentId,
            TotalReplies = 0, // New comment has no replies
            User = new CommentDto.UserDto
            {
                UserId = user.UserId,
                OauthSub = user.OauthSub,
                Username = user.Username,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
                IsActive = user.IsActive,
                VerificationStatus = user.VerificationStatus
            }
        };

        return ApiResult.Ok(commentDto);
    }
}