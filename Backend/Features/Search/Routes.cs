using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Interfaces;
using Backend.Common.Helpers.Types;
using Backend.Features.Post.Queries.GetPostById;
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
                [FromQuery] string q,
                [FromQuery(Name = "cursor")] string? cursor,
                IMediator mediator,
                [FromQuery] int pageSize = 15
            ) =>
            {
                var query = new SearchPostsQuery
                {
                    Query = q,
                    PaginationMeta = new InfiniteCursorPaginationMeta
                    {
                        Cursor = cursor,
                        PageSize = pageSize
                    }
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
            .Produces<InfiniteCursorPage<GetPostByIdResponse>>()
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(503)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Search posts using ML-powered hybrid search";
                operation.Description =
                    "Search for posts using semantic and keyword search capabilities with cursor-based pagination";
                operation.Parameters[0].Description = "Search query string";
                operation.Parameters[1].Description = "Cursor for pagination (optional)";
                operation.Parameters[2].Description = "Number of results per page (1-100, default: 15)";
                return operation;
            });
    }
}