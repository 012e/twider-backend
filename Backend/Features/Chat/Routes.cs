using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers;
using Backend.Common.Helpers.Interfaces;
using Backend.Common.Helpers.Types;
using Backend.Features.Chat.Commands.SendMessage;
using Backend.Features.Chat.DTOs;
using Backend.Features.Chat.Queries.GetChatMessages;
using Backend.Features.Chat.Queries.GetChats;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Chat;

public class Routes : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("chat")
            .WithTags("Chat")
            .RequireAuthorization();

        // Get list of chats for current user
        group.MapGet("", async ([FromQuery] string? cursor, [FromQuery] int pageSize, IMediator mediator) =>
            {
                var query = new GetChatsQuery
                {
                    PaginationMeta = new InfiniteCursorPaginationMeta
                    {
                        Cursor = cursor,
                        PageSize = pageSize > 0 ? pageSize : 20
                    }
                };

                Validator.ValidateObject(query, new ValidationContext(query), validateAllProperties: true);
                var result = await mediator.Send(query);

                if (result.IsFailed)
                {
                    return result.ToErrorResponse();
                }

                return Results.Ok(result.Value);
            })
            .WithName("GetChats")
            .Produces<InfiniteCursorPage<ChatDto>>()
            .Produces<ProblemDetails>(401);

        // Send a direct message
        group.MapPost("dm/{otherUserId:guid}", async ([FromRoute] Guid otherUserId, [FromBody] SendMessageRequest request, IMediator mediator) =>
            {
                var command = new SendMessageCommand
                {
                    OtherUserId = otherUserId,
                    Content = request.Content
                };

                Validator.ValidateObject(command, new ValidationContext(command), validateAllProperties: true);
                var result = await mediator.Send(command);

                if (result.IsFailed)
                {
                    return result.ToErrorResponse();
                }

                return Results.Ok(result.Value);
            })
            .WithName("SendDirectMessage")
            .Produces<ItemId>(201)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(404);

        // Get messages for a direct message conversation
        group.MapGet("dm/{otherUserId:guid}/messages", async ([FromRoute] Guid otherUserId, [FromQuery] string? before, [FromQuery] string? after, [FromQuery] int pageSize, IMediator mediator) =>
            {
                var query = new GetChatMessagesQuery
                {
                    OtherUserId = otherUserId,
                    Before = before,
                    After = after,
                    PageSize = pageSize > 0 ? pageSize : 50
                };

                Validator.ValidateObject(query, new ValidationContext(query), validateAllProperties: true);
                var result = await mediator.Send(query);

                if (result.IsFailed)
                {
                    return result.ToErrorResponse();
                }

                return Results.Ok(result.Value);
            })
            .WithName("GetDirectMessages")
            .Produces<InfiniteCursorPage<MessageDto>>()
            .Produces<ProblemDetails>(401);
    }
}

public class SendMessageRequest
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Content { get; set; } = null!;
}
