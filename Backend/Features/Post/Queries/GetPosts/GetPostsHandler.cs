using Backend.Common.DbContext;
using Backend.Common.Helpers;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using Backend.Features.Post.Mappers;
using Backend.Features.Post.Queries.GetPostById;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Post.Queries.GetPosts;

public class GetPostsHandler : IRequestHandler<GetPostsQuery, ApiResult<InfiniteCursorPage<GetPostByIdResponse>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public GetPostsHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<InfiniteCursorPage<GetPostByIdResponse>>> Handle(GetPostsQuery request,
        CancellationToken cancellationToken)
    {
        var cursor = request.PaginationMeta.Cursor;
        var pageSize = request.PaginationMeta.PageSize;
        var currentUserId = _currentUserService.User!.UserId;

        // Build the base query
        var baseQuery = _db.Posts
            .Include(post => post.User)
            .Include(post => post.Media)
            .OrderByDescending(b => b.CreatedAt)
            .ThenBy(b => b.PostId);

        // Apply cursor filter if provided
        IQueryable<Backend.Common.DbContext.Post.Post> filteredQuery = baseQuery;
        if (!string.IsNullOrEmpty(cursor))
        {
            try
            {
                var decodedCursor = CursorEncoder.Decode(cursor);
                var cursorPostId = Guid.Parse(decodedCursor);
                
                // Find the cursor post to get its CreatedAt timestamp
                var cursorPost = await _db.Posts
                    .Where(p => p.PostId == cursorPostId)
                    .Select(p => new { p.CreatedAt, p.PostId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (cursorPost != null)
                {
                    // Filter posts that are older than the cursor post (descending order)
                    filteredQuery = baseQuery.Where(p => 
                        p.CreatedAt < cursorPost.CreatedAt || 
                        (p.CreatedAt == cursorPost.CreatedAt && p.PostId.CompareTo(cursorPost.PostId) > 0));
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
        
        // Generate next cursor from the last item
        var nextCursor = itemsToReturn.Any() 
            ? CursorEncoder.Encode(itemsToReturn.Last().PostId.ToString()) 
            : null;

        var result = new InfiniteCursorPage<GetPostByIdResponse>(
            items: itemsToReturn.ToList(),
            nextCursor: nextCursor,
            hasMore: hasMore
        );

        return ApiResult.Ok(result);
    }
}