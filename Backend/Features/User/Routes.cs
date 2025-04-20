using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers;
using Backend.Common.Helpers.Interfaces;
using Backend.Features.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.User;

public class Routes : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id}", async (int id, IMediator mediator) =>
                {
                    var query = new GetUserByIdQuery
                    {
                        Id = id
                    };
                    Validator.ValidateObject(query, new ValidationContext(query), validateAllProperties: true);
                    return await mediator.Send(query);
                }
            )
            .WithName("GetUserById")
            .RequireAuthorization()
            .Produces<GetUserByIdResponse>()
            .Produces<ProblemDetails>(404);
    }
}