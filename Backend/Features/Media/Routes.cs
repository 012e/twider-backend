using Backend.Common.Helpers.Interfaces;
using Backend.Features.Media.Commands;
using MediatR;

namespace Backend.Features.Media;

public class Routes : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/generate-medium-url", async (IMediator mediator) =>
        {

            var response = await mediator.Send(new GenerateUploadUrlCommand());
            if (response.IsFailed)
            {
                return response.ToErrorResponse();
            }

            return Results.Ok(response.Value);

        })
        .RequireAuthorization()
        .Produces<GenerateUploadUrlResponse>(201);
    }
}