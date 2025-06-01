using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Interfaces;
using Backend.Features.Search.Queries.SearchPosts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Search;

public class Routes : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var search = app
            .MapGroup("search")
            .WithTags("Search");

        search.MapGet("posts", async (
                IMediator mediator,
                [FromQuery] string q,
                [FromQuery] int offset = 0,
                [FromQuery] int limit = 15
            ) =>
            {
                var query = new SearchPostsQuery
                {
                    Query = q,
                    Offset = offset,
                    Limit = limit
                };

                Validator.ValidateObject(query, new ValidationContext(query), true);

                var response = await mediator.Send(query);

                if (response.IsFailed)
                {
                    return response.ToErrorResponse();
                }

                return Results.Ok(response.Value);
            })
            .WithName("SearchPosts")
            .RequireAuthorization()
            .Produces<SearchPostsResponse>()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(503)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Search posts using ML-powered hybrid search";
                operation.Description = "Search for posts using semantic and keyword search capabilities";
                operation.Parameters[0].Description = "Search query string";
                operation.Parameters[1].Description = "Number of results to skip for pagination";
                operation.Parameters[2].Description = "Maximum number of results to return (1-100)";
                return operation;
            });
    }
}