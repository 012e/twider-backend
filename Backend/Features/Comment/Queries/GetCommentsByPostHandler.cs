using Backend.Common.DbContext;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Common.Helpers.Types;
using Backend.Common.Helpers;

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

        var post = await _db.Posts
            .FirstOrDefaultAsync(p => p.PostId == request.PostId, cancellationToken);
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
        // Build the base query
        var baseQuery = _db.Comments
            .Include(o => o.User)
            .Where(c => c.PostId == post.PostId)
            .Where(c => c.ParentCommentId == commentId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.User.CreatedAt)
            .ThenByDescending(p => p.CommentId);

        // Apply cursor filter if provided
        IQueryable<Common.DbContext.Post.Comment> filteredQuery = baseQuery;
        if (!string.IsNullOrEmpty(meta.Cursor))
        {
            try
            {
                var decodedCursor = CursorEncoder.Decode(meta.Cursor);
                var cursorCommentId = Guid.Parse(decodedCursor);
                
                // Find the cursor comment to get its timestamps
                var cursorComment = await _db.Comments
                    .Include(c => c.User)
                    .Where(c => c.CommentId == cursorCommentId)
                    .Select(c => new { c.CreatedAt, UserCreatedAt = c.User.CreatedAt, c.CommentId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (cursorComment != null)
                {
                    // Filter comments that are older than the cursor comment (descending order)
                    filteredQuery = baseQuery.Where(c => 
                        c.CreatedAt < cursorComment.CreatedAt || 
                        (c.CreatedAt == cursorComment.CreatedAt && c.User.CreatedAt < cursorComment.UserCreatedAt) ||
                        (c.CreatedAt == cursorComment.CreatedAt && c.User.CreatedAt == cursorComment.UserCreatedAt && c.CommentId.CompareTo(cursorComment.CommentId) < 0));
                }
            }
            catch (Exception)
            {
                // Invalid cursor format, ignore and start from beginning
            }
        }

        // Take one extra item to determine if there are more pages
        var comments = await filteredQuery
            .Take(meta.PageSize + 1)
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
            })
            .ToListAsync(cancellationToken);

        // Determine if there are more items and prepare the result
        var hasMore = comments.Count > meta.PageSize;
        var itemsToReturn = hasMore ? comments.Take(meta.PageSize).ToList() : comments;
        
        // Generate next cursor from the last item
        var nextCursor = itemsToReturn.Any() 
            ? CursorEncoder.Encode(itemsToReturn.Last().CommentId.ToString()) 
            : null;

        return new InfiniteCursorPage<CommentDto>(
            items: itemsToReturn.ToList(),
            nextCursor: nextCursor,
            hasMore: hasMore
        );
    }
}