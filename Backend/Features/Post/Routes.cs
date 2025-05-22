using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Interfaces;
using Backend.Common.Helpers.Types;
using Backend.Features.Post.Commands.AddReaction;
using Backend.Features.Post.Commands.CreatePost;
using Backend.Features.Post.Commands.DeletePost;
using Backend.Features.Post.Commands.DeleteReaction;
using Backend.Features.Post.Commands.UpdatePost;
using Backend.Features.Post.Queries;
using Backend.Features.Post.Queries.GetPostById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Post;

public class Routes : IEndPoint
{
    public class ReactionTypeDto
    {
        public string ReactionType { get; set; } = null!;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var post = app
            .MapGroup("posts")
            .WithTags("Posts");

        var reaction = app
            .MapGroup("posts")
            .WithTags("Post reactions");

        post.MapGet("{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetPostByIdQuery(id));

                if (response.IsFailed)
                {
                    return response.ToErrorResponse();
                }

                return Results.Ok(response.Value);
            })
            .WithName("GetPostById")
            .Produces<GetPostByIdResponse>()
            .Produces<ProblemDetails>(404);

        post.MapPost("", async ([FromBody] CreatePostCommand request, IMediator mediator) =>
            {
                var response = await mediator.Send(request);

                if (response.IsFailed)
                {
                    return response.ToErrorResponse();
                }

                return Results.Created($"/posts/{response.Value}", response.Value);
            })
            .WithName("CreatePost")
            .RequireAuthorization()
            .Produces<ItemId>(201)
            .Produces<ProblemDetails>(400);

        post.MapDelete("{id}", async ([FromRoute] Guid id, IMediator mediator) =>
            {
                var response = await mediator.Send(new DeletePostCommand(id));
                if (response.IsFailed)
                {
                    return response.ToErrorResponse();
                }

                return Results.NoContent();
            })
            .WithName("DeletePost")
            .Produces(204)
            .Produces<ProblemDetails>(404);

        post.MapGet("",
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
            .WithName("GetPosts")
            .RequireAuthorization()
            .Produces<InfiniteCursorPage<GetPostByIdResponse>>()
            .Produces<ProblemDetails>(400);

        post.MapPut("{id:guid}",
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
            .WithName("UpdatePost")
            .Produces<ProblemDetails>(400)
            .Produces<Unit>(204)
            .Produces<ProblemDetails>(404);

        reaction.MapPost("{id:guid}/react", async
                ([FromRoute] Guid id, IMediator mediator, [FromBody] ReactionTypeDto reactionType) =>
            {
                ReactionTypeHelper.Validate(reactionType.ReactionType);

                var command = new PostReactionCommand
                {
                    PostId = id,
                    ReactionType = new PostReactionCommand.ReactionDto
                    {
                        ReactionType = reactionType.ReactionType.ToReactionTypeEnum()
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
            .WithName("AddReactionToPost")
            .Produces<ProblemDetails>(400)
            .Produces<Unit>(204)
            .Produces<ProblemDetails>(404);

        reaction.MapDelete("{id:guid}/react", async
                ([FromRoute] Guid id, IMediator mediator) =>
            {
                var command = new RemoveReactionCommand
                {
                    PostId = id,
                };
                Validator.ValidateObject(command, new ValidationContext(command), true);

                var response = await mediator.Send(command);
                return response.IsFailed ? response.ToErrorResponse() : Results.NoContent();
            })
            .WithName("RemoveReactionFromPost")
            .Produces<Unit>(204)
            .Produces<ProblemDetails>(404);
    }
}