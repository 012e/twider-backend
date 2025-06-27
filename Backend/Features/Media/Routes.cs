using Backend.Common.Helpers.Interfaces;
using Backend.Features.Media.Commands;
using MediatR;

namespace Backend.Features.Media;

public class Routes : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("media")
            .WithTags("Media");
        group.MapPost("/generate-medium-url", async (GenerateUploadUrlCommand command, IMediator mediator) =>
            {
                var response = await mediator.Send(command);
                if (response.IsFailed)
                {
                    return response.ToErrorResponse();
                }

                return Results.Created("<irrelevant>", response.Value);
            })
            .RequireAuthorization()
            .Produces<GenerateUploadUrlResponse>(201);
    }
}