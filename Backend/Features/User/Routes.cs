using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers;
using Backend.Common.Helpers.Interfaces;
using Backend.Common.Helpers.Types;
using Backend.Features.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.User;

public class Routes : IEndPoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("users")
            .WithTags("Users");

        group.MapGet("{id:guid}", async (Guid id, IMediator mediator) =>
                {
                    var query = new GetUserByIdQuery
                    {
                        Id = id
                    };
                    Validator.ValidateObject(query, new ValidationContext(query), validateAllProperties: true);
                    var result = await mediator.Send(query);
                    
                    if (result.IsFailed)
                    {
                        return result.ToErrorResponse();
                    }
                    
                    return Results.Ok(result.Value);
                }
            )
            .WithName("GetUserById")
            .RequireAuthorization()
            .Produces<GetUserByIdResponse>()
            .Produces<ProblemDetails>(404);

        group.MapGet("current",
                async (IMediator mediator) => await mediator.Send(new GetCurrentLoggedInUserQuery()))
            .WithName("GetCurrentUser")
            .RequireAuthorization()
            .Produces<GetUserByIdResponse>()
            .Produces<ProblemDetails>(404);
    }
}