using Backend.Common.DbContext;
using Backend.Common.Helpers;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using Backend.Features.Post.Mappers;
using Backend.Features.Post.Queries.GetPostById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Post.Queries.GetPostsByUser;

public class
    GetPostByUserHandler : IRequestHandler<GetPostByUserQuery,
    ApiResult<InfiniteCursorPage<GetPostByIdResponse>>> // <<<>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public GetPostByUserHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<InfiniteCursorPage<GetPostByIdResponse>>> Handle(GetPostByUserQuery request,
        CancellationToken cancellationToken)
    {
        var cursor = request.PaginationMeta.Cursor;
        var pageSize = request.PaginationMeta.PageSize;
        var currentUserId = _currentUserService.User!.UserId;
        var userExists =
            await _db.Users.AnyAsync(u => u.UserId == request.UserId, cancellationToken: cancellationToken);
        if (!userExists)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"User with id {request.UserId} not found.",
                Status = 404,
            });
        }

        // Build the base query (order by newest first to match test expectations)
        var baseQuery = _db.Posts
            .Include(post => post.User)
            .Include(post => post.Media)
            .Where(b => b.UserId == request.UserId)
            .OrderByDescending(b => b.CreatedAt)
            .ThenBy(b => b.PostId);

        // Apply cursor filter if provided
        IQueryable<Backend.Common.DbContext.Post.Post> filteredQuery = baseQuery;
        if (!string.IsNullOrEmpty(cursor))
        {
            try
            {
                var decodedCursor = CursorEncoder.Decode(cursor);
                var cursorParts = decodedCursor.Split('|');
                if (cursorParts.Length == 2)
                {
                    var cursorCreatedAt = DateTime.Parse(cursorParts[0], null, System.Globalization.DateTimeStyles.RoundtripKind);
                    var cursorPostId = Guid.Parse(cursorParts[1]);
                    
                    // Filter posts that are older than the cursor (descending order)
                    filteredQuery = baseQuery.Where(p => 
                        p.CreatedAt < cursorCreatedAt || 
                        (p.CreatedAt == cursorCreatedAt && p.PostId.CompareTo(cursorPostId) > 0));
                }
            }
            catch (Exception)
            {
                // Invalid cursor format, ignore and start from beginning
            }
        }

        // Take one extra item to determine if there are more pages
        var posts = await filteredQuery
            .Take(pageSize + 1)
            .Select(b => new
            {
                Post = b,
                UserReaction = b.Reactions.FirstOrDefault(x => x.UserId == currentUserId)
            })
            .Select(b => new GetPostByIdResponse
            {
                PostId = b.Post.PostId,
                Content = b.Post.Content,
                CreatedAt = b.Post.CreatedAt,
                User = b.Post.User.ToUserDto(),
                UpdatedAt = b.Post.UpdatedAt,
                Reactions = b.Post.Reactions.ExtractReactionCount(),
                ReactionCount = b.Post.Reactions.Count(),
                CommentCount = b.Post.Comments.Count(),
                UserReaction = b.UserReaction == null ? null : b.UserReaction.ReactionType.ToFriendlyString(),
                MediaUrls = b.Post.Media.Select(r => r.Url).ToList(),
            })
            .ToListAsync(cancellationToken);

        // Determine if there are more items and prepare the result
        var hasMore = posts.Count > pageSize;
        var itemsToReturn = hasMore ? posts.Take(pageSize).ToList() : posts;
        
        // Generate next cursor from the last item (CreatedAt|PostId format)
        var nextCursor = itemsToReturn.Any() 
            ? CursorEncoder.Encode($"{itemsToReturn.Last().CreatedAt:O}|{itemsToReturn.Last().PostId}") 
            : null;

        var result = new InfiniteCursorPage<GetPostByIdResponse>(
            items: itemsToReturn.ToList(),
            nextCursor: nextCursor,
            hasMore: hasMore
        );

        return ApiResult.Ok(result);
    }
}