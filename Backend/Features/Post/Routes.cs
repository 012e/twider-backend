using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Interfaces;
using Backend.Common.Helpers.Types;
using Backend.Features.Post.Commands;
using Backend.Features.Post.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Post;

public class Routes : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("posts")
            .WithTags("Posts");

        group.MapPost("", async ([FromBody] CreatePostCommand request, IMediator mediator) =>
            {
                var response = await mediator.Send(request);

                if (response.IsFailed)
                {
                    return response.ToErrorResponse();
                }

                return Results.Created($"/posts/{response.Value}", response.Value);
            })
            .RequireAuthorization()
            .Produces<ItemId>(201)
            .Produces<ProblemDetails>(400);

        group.MapDelete("{id}", async ([FromRoute] Guid id, IMediator mediator) =>
            {
                var response = await mediator.Send(new DeletePostCommand(id));
                if (response.IsFailed)
                {
                    return response.ToErrorResponse();
                }
                return Results.NoContent();
            })
            .Produces(204)
            .Produces<ProblemDetails>(404);

        group.MapGet("",
                async ([FromQuery(Name = "cursor")] string? cursor, IMediator mediator, [FromQuery] int pageSize = 10) =>
                {
                    var request = new GetPostsQuery
                    {
                        PaginationMeta = new InfiniteCursorPaginationMeta
                        {
                            Cursor = cursor,
                            PageSize = pageSize
                        }
                    };

                    Validator.ValidateObject(request, new ValidationContext(request), true);

                    var response = await mediator.Send(request);

                    if (response.IsFailed)
                    {
                        return response.ToErrorResponse();
                    }

                    return Results.Ok(response.Value);
                })
            .Produces<InfiniteCursorPage<GetPostByIdResponse>>()
            .Produces<ProblemDetails>(400);
    }
}