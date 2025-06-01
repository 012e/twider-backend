using System.Text.Json;
using Backend.Common.DbContext;
using Backend.Common.Helpers.Types;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Search.Queries.SearchPosts;

public class SearchPostsHandler : IRequestHandler<SearchPostsQuery, ApiResult<SearchPostsResponse>>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SearchPostsHandler> _logger;
    private readonly ApplicationDbContext _db;

    public SearchPostsHandler(IHttpClientFactory httpClientFactory, ILogger<SearchPostsHandler> logger, ApplicationDbContext db)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _db = db;
    }

    public async Task<ApiResult<SearchPostsResponse>> Handle(SearchPostsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("MlSearchClient");
            
            // Build query string parameters
            var queryParams = $"q={Uri.EscapeDataString(request.Query)}&offset={request.Offset}&limit={request.Limit}";
            var requestUri = $"/api/v1/search?{queryParams}";

            _logger.LogInformation("Calling ML search service: {RequestUri}", requestUri);

            var response = await httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("ML search service returned error: {StatusCode} - {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                
                return ApiResult<SearchPostsResponse>.Fail(new ProblemDetails
                {
                    Title = "Search service unavailable",
                    Detail = "The search service is currently unavailable. Please try again later.",
                    Status = 503
                });
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var searchResponse = JsonSerializer.Deserialize<SearchPostsResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (searchResponse == null)
            {
                _logger.LogError("Failed to deserialize search response");
                return ApiResult<SearchPostsResponse>.Fail(new ProblemDetails
                {
                    Title = "Search failed",
                    Detail = "Failed to process search results.",
                    Status = 500
                });
            }

            _logger.LogInformation("Search completed successfully. Found {Count} results", searchResponse.Results.Count);
            return ApiResult.Ok(searchResponse);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error when calling ML search service");
            return ApiResult<SearchPostsResponse>.Fail(new ProblemDetails
            {
                Title = "Search service unavailable",
                Detail = "Unable to connect to the search service.",
                Status = 503
            });
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout when calling ML search service");
            return ApiResult<SearchPostsResponse>.Fail(new ProblemDetails
            {
                Title = "Search timeout",
                Detail = "The search request timed out. Please try again.",
                Status = 408
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during search");
            return ApiResult<SearchPostsResponse>.Fail(new ProblemDetails
            {
                Title = "Search failed",
                Detail = "An unexpected error occurred during search.",
                Status = 500
            });
        }
    }
}