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

        group.MapGet("{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetPostByIdQuery(id));

                if (response.IsFailed)
                {
                    return response.ToErrorResponse();
                }

                return Results.Ok(response.Value);
            })
            .Produces<GetPostByIdResponse>()
            .Produces<ProblemDetails>(404);

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
                async ([FromQuery(Name = "cursor")] string? cursor, IMediator mediator,
                    [FromQuery] int pageSize = 10) =>
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

        group.MapPut("{id:guid}",
                async (IMediator mediator, [FromRoute] Guid id, [FromBody] UpdatePostCommand.UpdateContent request) =>
                {
                    Validator.ValidateObject(request, new ValidationContext(request), true);
                    var command = new UpdatePostCommand
                    {
                        PostId = id,
                        Content = request,
                    };

                    var response = await mediator.Send(command);
                    if (response.IsFailed)
                    {
                        return response.ToErrorResponse();
                    }

                    return Results.NoContent();
                })
            .Produces<ProblemDetails>(400)
            .Produces<Unit>(204)
            .Produces<ProblemDetails>(404);

        group.MapPost("{id:guid}/react", async
                ([FromRoute] Guid id, IMediator mediator, [FromBody] string reactionType) =>
            {
                ReactionTypeHelper.Validate(reactionType);

                var command = new PostReactionCommand
                {
                    PostId = id,
                    ReactionType = new PostReactionCommand.ReactionDto
                    {
                        ReactionType = reactionType.ToReactionTypeEnum()
                    }
                };
                Validator.ValidateObject(command, new ValidationContext(command), true);

                var response = await mediator.Send(command);
                if (response.IsFailed)
                {
                    return response.ToErrorResponse();
                }

                return Results.NoContent();
            })
            .Produces<ProblemDetails>(400)
            .Produces<Unit>(204)
            .Produces<ProblemDetails>(404);
    }
}