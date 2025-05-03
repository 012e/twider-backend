using Backend.Common.Helpers.Interfaces;
using Backend.Common.Helpers.Types;
using Backend.Features.Post.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Post;

public class Routes : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/posts", async ([FromBody] CreatePostCommand request, IMediator mediator) =>
            {
                var response = await mediator.Send(request);

                if (response.IsFailed)
                {
                    return response.ToIResult();
                }

                return Results.Created($"/posts/{response.Value}", response.Value);
            })
            .RequireAuthorization()
            .Produces<CreatedId>(201)
            .Produces<ProblemDetails>(400);
    }
}