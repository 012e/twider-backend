using System.Security.Claims;
using Backend.Common.Helpers;
using Backend.Common.Helpers.Interfaces;
using Backend.Features.Post.Commands;
using Backend.Features.User.Queries;
using MediatR;

namespace Backend.Features.Post;

using Microsoft.AspNetCore.Mvc;

public class Routes : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/posts", async ([FromBody] CreatePostCommand request, IMediator mediator) => await mediator.Send(request))
            .RequireAuthorization()
            .Produces<CreatePostResponse>()
            .Produces<ProblemDetails>(400);
    }
}