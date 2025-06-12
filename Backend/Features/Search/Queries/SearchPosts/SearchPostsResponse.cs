using Backend.Common.Helpers.Types;
using Backend.Features.Post.Queries.GetPostById;

namespace Backend.Features.Search.Queries.SearchPosts;

public class SearchPostsResponse : InfiniteCursorPage<GetPostByIdResponse>
{
    public SearchPostsResponse(
        IReadOnlyList<GetPostByIdResponse> items,
        string? nextCursor,
        bool hasMore) : base(items, nextCursor, hasMore)
    {
    }
}