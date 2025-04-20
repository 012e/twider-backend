using Backend.Common.DbContext;
using Backend.Common.Services;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Post.Commands;

public class CreatePostHandler : IRequestHandler<CreatePostCommand, ApiResult<CreatePostResponse>>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public CreatePostHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<CreatePostResponse>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var user = _currentUserService.User;
        if (user == null)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "User not found",
                Detail = $"User with ID {user} not found."
            });
        }

        return ApiResult.Ok(new CreatePostResponse(Id: 3223));
    }
}