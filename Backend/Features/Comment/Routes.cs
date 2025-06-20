using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Interfaces;
using Backend.Common.Helpers.Types;
using Backend.Features.Comment.Commands.AddReaction;
using Backend.Features.Comment.Commands.CreateComment;
using Backend.Features.Comment.Commands.DeleteComment;
using Backend.Features.Comment.Commands.DeleteReaction;
using Backend.Features.Comment.Commands.UpdateComment;
using Backend.Features.Comment.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Comment;

public class Routes : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("").WithTags("Comments");

        group.MapPost("/posts/{postId:guid}/comments/{parentCommentId:guid}",
                async ([FromRoute] Guid postId, [FromRoute] Guid? parentCommentId,
                    [FromBody] CommentContent content, IMediator mediator) =>
                {
                    var command = new CreateCommentCommand
                    {
                        PostId = postId,
                        ParentCommentId = parentCommentId,
                        Content = content
                    };

                    var response = await mediator.Send(command);
                    if (response.IsFailed)
                    {
                        return response.ToErrorResponse();
                    }

                    return Results.Created($"/posts/{command.PostId}/comments/{response.Value.CommentId}", response.Value);
                })
            .WithName("CreateCommentWithParent")
            .RequireAuthorization()
            .Produces<CommentDto>(201)
            .Produces<ProblemDetails>(404)
            .Produces<ProblemDetails>(400);

        group.MapPost("/posts/{postId:guid}/comments/",
                async ([FromRoute] Guid postId,
                    [FromBody] CommentContent content, IMediator mediator) =>
                {
                    var command = new CreateCommentCommand
                    {
                        PostId = postId,
                        ParentCommentId = null,
                        Content = content
                    };

                    var response = await mediator.Send(command);
                    if (response.IsFailed)
                    {
                        return response.ToErrorResponse();
                    }

                    return Results.Created($"/posts/{command.PostId}/comments/{response.Value.CommentId}", response.Value);
                })
            .WithName("CreateComment")
            .RequireAuthorization()
            .Produces<CommentDto>(201)
            .Produces<ProblemDetails>(404)
            .Produces<ProblemDetails>(400);

        group.MapGet("/posts/{postId:guid}/comments/{commentId:guid}", async (
                [FromRoute] Guid postId,
                [FromRoute] Guid commentId,
                [FromQuery] string? cursor,
                IMediator mediator,
                [FromQuery] int pageSize = 10) =>
            {
                var command = new GetCommentsByPostQuery(postId, commentId, new InfiniteCursorPaginationMeta
                {
                    Cursor = cursor,
                    PageSize = pageSize
                });
                var response = await mediator.Send(command);

                if (response.IsFailed)
                {
                    return response.ToErrorResponse();
                }

                return Results.Ok(response.Value);
            })
            .WithName("GetCommentsByPostAndCommentId")
            .Produces<CommentDto>()
            .Produces<ProblemDetails>(404);

        // Workaround. OpenAPI does not support optional parameters in routes
        group.MapGet("/posts/{postId:guid}/comments/", async ([FromRoute] Guid postId, [FromQuery] string? cursor,
                IMediator mediator, [FromQuery] int pageSize = 10) =>
            {
                var command = new GetCommentsByPostQuery(postId, null, new InfiniteCursorPaginationMeta
                {
                    Cursor = cursor,
                    PageSize = pageSize
                });
                var response = await mediator.Send(command);

                if (response.IsFailed)
                {
                    return response.ToErrorResponse();
                }

                return Results.Ok(response.Value);
            })
            .WithName("GetCommentsByPostId")
            .Produces<CommentDto>()
            .Produces<ProblemDetails>(404);

        group.MapPut("/posts/{postId:guid}/comments/{commentId:guid}",
                async ([FromRoute] Guid postId, [FromRoute] Guid commentId,
                    [FromBody] CommentContent content, IMediator mediator) =>
                {
                    var command = new UpdateCommentCommand
                    {
                        PostId = postId,
                        CommentId = commentId,
                        Content = content
                    };

                    var response = await mediator.Send(command);
                    if (response.IsFailed)
                    {
                        return response.ToErrorResponse();
                    }

                    return Results.NoContent();
                })
            .WithName("UpdateComment")
            .RequireAuthorization()
            .Produces(204)
            .Produces<ProblemDetails>(404)
            .Produces<ProblemDetails>(400);

        group.MapDelete("/posts/{postId:guid}/comments/{commentId:guid}",
                async ([FromRoute] Guid postId, [FromRoute] Guid commentId,
                    IMediator mediator) =>
                {
                    var command = new DeleteCommentCommand
                    {
                        PostId = postId,
                        CommentId = commentId
                    };

                    var response = await mediator.Send(command);
                    if (response.IsFailed)
                    {
                        return response.ToErrorResponse();
                    }

                    return Results.NoContent();
                })
            .WithName("DeleteComment")
            .RequireAuthorization()
            .Produces(204)
            .Produces<ProblemDetails>(404)
            .Produces<ProblemDetails>(400);

        group.MapPost("posts/{postId:guid}/comments/{commentId:guid}/reactions", async ([FromRoute] Guid postId,
                [FromRoute] Guid commentId,
                [FromBody] ReactionTypeDto reactionType, IMediator mediator) =>
            {
                var command = new AddReactionCommand
                {
                    CommentId = commentId,
                    PostId = postId,
                    ReactionType = new AddReactionCommand.ReactionDto
                    {
                        ReactionType = reactionType.ReactionType.ToReactionTypeEnum()
                    }
                };

                var response = await mediator.Send(command);
                if (response.IsFailed)
                {
                    return response.ToErrorResponse();
                }

                return Results.Created($"/posts/{postId}/comments/{commentId}/reactions", response.Value);
            })
            .WithName("AddCommentReaction")
            .RequireAuthorization()
            .Produces(201)
            .Produces<ProblemDetails>(404)
            .Produces<ProblemDetails>(400);

        group.MapDelete("posts/{postId:guid}/comments/{commentId:guid}/reactions", async (
                [FromRoute] Guid postId,
                [FromRoute] Guid commentId, IMediator mediator) =>
            {
                var command = new DeleteReactionCommand
                {
                    PostId = postId,
                    CommentId = commentId
                };

                var response = await mediator.Send(command);
                if (response.IsFailed)
                {
                    return response.ToErrorResponse();
                }

                return Results.NoContent();
            })
            .WithName("RemoveCommentReaction")
            .RequireAuthorization()
            .Produces(204)
            .Produces<ProblemDetails>(404);
    }

    public class ReactionTypeDto
    {
        [Required]
        [RegularExpression("$(like|love|haha|wow|sad|care)^")]
        public string ReactionType { get; set; } = null!;
    }
}