using Backend.Common.Helpers.Interfaces;

namespace Backend.Features.Health;

public class Routes : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/health/hello", () => "OK");
    }
}