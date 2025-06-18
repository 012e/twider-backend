using System.Text.Json;
using Backend.Common.DbContext;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using Backend.Features.Post.Mappers;
using Backend.Features.Post.Queries.GetPostById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Search.Queries.SearchPosts;

public class SearchPostsHandler : IRequestHandler<SearchPostsQuery, ApiResult<InfiniteCursorPage<GetPostByIdResponse>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SearchPostsHandler> _logger;

    public SearchPostsHandler(
        IHttpClientFactory httpClientFactory,
        ILogger<SearchPostsHandler> logger,
        ApplicationDbContext db,
        ICurrentUserService currentUserService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<InfiniteCursorPage<GetPostByIdResponse>>> Handle(SearchPostsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("MlSearchClient");

            // Convert cursor-based pagination to offset/limit for ML service
            var offset = 0;
            var limit = request.PaginationMeta.PageSize;

            // For cursor-based pagination, we'll need to fetch from beginning and filter
            // This is a simplified approach - in production you might want to cache or use different strategy
            if (!string.IsNullOrEmpty(request.PaginationMeta.Cursor))
            {
                // Parse cursor to get offset (simplified - you might want to use proper cursor encoding)
                if (int.TryParse(request.PaginationMeta.Cursor, out var cursorOffset))
                {
                    offset = cursorOffset;
                }
            }

            // Build query string parameters
            var queryParams =
                $"q={Uri.EscapeDataString(request.Query)}&offset={offset}&limit={limit + 1}"; // +1 to check hasMore
            var requestUri = $"/api/v1/search?{queryParams}";

            _logger.LogInformation("Calling ML search service: {RequestUri}", requestUri);

            var response = await httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("ML search service returned error: {StatusCode} - {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);

                return ApiResult<InfiniteCursorPage<GetPostByIdResponse>>.Fail(new ProblemDetails
                {
                    Title = "Search service unavailable",
                    Detail = "The search service is currently unavailable. Please try again later.",
                    Status = 503
                });
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var mlSearchResponse = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (mlSearchResponse == null)
            {
                _logger.LogError("Failed to deserialize search response");
                return ApiResult<InfiniteCursorPage<GetPostByIdResponse>>.Fail(new ProblemDetails
                {
                    Title = "Search failed",
                    Detail = "Failed to process search results.",
                    Status = 500
                });
            }

            // Extract post IDs from ML search results
            var allPostIds = mlSearchResponse.Results
                .Select(r => Guid.TryParse(r.Id, out var guid) ? guid : (Guid?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            if (!allPostIds.Any())
            {
                _logger.LogInformation("No valid post IDs found in search results");
                return ApiResult.Ok(new InfiniteCursorPage<GetPostByIdResponse>(
                    [],
                    null,
                    false
                ));
            }

            // Take only the requested page size and check if there are more
            var hasMore = allPostIds.Count > limit;
            var postIds = allPostIds.Take(limit).ToList();

            // Query database for full post data using the IDs from ML search
            var currentUserId = _currentUserService.User!.UserId;

            var dbPosts = await _db.Posts
                .Include(post => post.User)
                .Include(post => post.Media)
                .Include(post => post.Reactions)
                .Include(post => post.Comments)
                .Where(p => postIds.Contains(p.PostId))
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
                    MediaUrls = b.Post.Media.Select(r => r.Url!).ToList(),
                })
                .ToListAsync(cancellationToken);

            // Preserve the order from ML search results
            var orderedResults = postIds
                .Select(id => dbPosts.FirstOrDefault(p => p.PostId == id))
                .Where(p => p != null)
                .ToList();

            // Generate next cursor if there are more results
            var nextCursor = hasMore ? (offset + limit).ToString() : null;

            var searchResponse = new InfiniteCursorPage<GetPostByIdResponse>(
                orderedResults!,
                nextCursor,
                hasMore
            );

            _logger.LogInformation("Search completed successfully. Found {Count} results", orderedResults.Count);
            return ApiResult.Ok(searchResponse);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error when calling ML search service");
            return ApiResult<InfiniteCursorPage<GetPostByIdResponse>>.Fail(new ProblemDetails
            {
                Title = "Search service unavailable",
                Detail = "Unable to connect to the search service.",
                Status = 503
            });
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout when calling ML search service");
            return ApiResult<InfiniteCursorPage<GetPostByIdResponse>>.Fail(new ProblemDetails
            {
                Title = "Search timeout",
                Detail = "The search request timed out. Please try again.",
                Status = 408
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during search");
            return ApiResult<InfiniteCursorPage<GetPostByIdResponse>>.Fail(new ProblemDetails
            {
                Title = "Search failed",
                Detail = "An unexpected error occurred during search.",
                Status = 500
            });
        }
    }
}

// Internal DTO for ML search service response
internal class SearchResponse
{
    public required List<SearchResult> Results { get; set; } = [];
    public required int Total { get; set; }
    public required int Offset { get; set; }
    public required int Limit { get; set; }
}

internal class SearchResult
{
    public required string Id { get; set; } = string.Empty;
    public required string Content { get; set; } = string.Empty;
    public required List<string>? MediaUrls { get; set; } = [];
}