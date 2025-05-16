using Backend.Common.DbContext;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;

namespace Backend.Features.Comment.Queries;

public class GetCommentsByPostHandler :
    IRequestHandler<GetCommentsByPostQuery, ApiResult<InfiniteCursorPage<CommentDto>>>
{
    private readonly ApplicationDbContext _db;

    public GetCommentsByPostHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResult<InfiniteCursorPage<CommentDto>>> Handle(GetCommentsByPostQuery request,
        CancellationToken cancellationToken)
    {
        var paginationMeta = request.Meta;

        var post = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == request.PostId, cancellationToken);
        if (post == null)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Post not found",
                Detail = $"Post with ID {request.PostId} not found."
            });
        }

        if (request.CommentId is null)
            return ApiResult.Ok(await GetReplies(paginationMeta, post, request.CommentId, cancellationToken));

        var comment = _db.Comments.FirstOrDefault(c => c.CommentId == request.CommentId);
        if (comment is null)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Post not found",
                Detail = $"Post with ID {request.PostId} not found."
            });
        }

        return ApiResult.Ok(await GetReplies(paginationMeta, post, request.CommentId, cancellationToken));
    }


    private async Task<InfiniteCursorPage<CommentDto>> GetReplies(InfiniteCursorPaginationMeta meta,
        Common.DbContext.Post.Post post, Guid? commentId,
        CancellationToken cancellationToken)
    {
        var query = _db.Comments
            .Include(o => o.User)
            .Where(c => c.PostId == post.PostId)
            .Where(c => c.ParentCommentId == commentId)
            .Select(c => new CommentDto
            {
                CommentId = c.CommentId,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                ParentCommentId = c.ParentCommentId,
                TotalReplies = _db.Comments.Count(comment => comment.ParentCommentId == c.CommentId),

                User = new CommentDto.UserDto
                {
                    UserId = c.User.UserId,
                    Username = c.User.Username,
                    ProfilePicture = c.User.ProfilePicture,
                    Email = c.User.Email,
                    Bio = c.User.Bio,
                    CreatedAt = c.User.CreatedAt,
                    IsActive = c.User.IsActive,
                    VerificationStatus = c.User.VerificationStatus,
                    LastLogin = c.User.LastLogin,
                    OauthSub = c.User.OauthSub,
                },
            });

        return await InfinitePaginationService.PaginateAsync(
            source: query,
            after: meta.Cursor,
            limit: meta.PageSize,
            keySelector: c => c.CommentId
        );
    }
}