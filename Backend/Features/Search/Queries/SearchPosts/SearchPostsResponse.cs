namespace Backend.Features.Search.Queries.SearchPosts;

public class SearchPostsResponse
{
    public required List<PostSearchResult> Results { get; set; } = [];
    public required int Total { get; set; }
    public required int Offset { get; set; }
    public required int Limit { get; set; }
}

public class PostSearchResult
{
    public required string Id { get; set; } = string.Empty;
    public required string Content { get; set; } = string.Empty;
    public required List<string> MediaUrls { get; set; } = [];
}