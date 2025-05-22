using Backend.Common.DbContext;
using Backend.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.User.Queries;

public class
    GetCurrentLoggedInUserHandler : IRequestHandler<GetCurrentLoggedInUserQuery,
    ApiResult<GetCurrentLoggedInUserResponse>>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentLoggedInUserHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<GetCurrentLoggedInUserResponse>> Handle(GetCurrentLoggedInUserQuery request,
        CancellationToken cancellationToken)
    {
        var currentUser = _currentUserService.User;
        if (currentUser == null)
        {
            throw new Exception("User not logged in");
        }

        var user = await _db.Users
            .Where(u => u.UserId == currentUser.UserId)
            .Select(u => new Common.DbContext.User
            {
                UserId = u.UserId,
                OauthSub = u.OauthSub,
                Username = u.Username,
                Email = u.Email,
                ProfilePicture = u.ProfilePicture,
                Bio = u.Bio,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin,
                IsActive = u.IsActive,
                VerificationStatus = u.VerificationStatus
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return ApiResult<GetCurrentLoggedInUserResponse>.Fail(new ProblemDetails
            {
                Title = "User not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"User with ID {user.UserId} not found."
            });
        }

        return ApiResult<GetCurrentLoggedInUserResponse>.Ok(new GetCurrentLoggedInUserResponse
            {
                UserId = user.UserId,
                OauthSub = user.OauthSub,
                Username = user.Username,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
                IsActive = user.IsActive,
                VerificationStatus = user.VerificationStatus
            }
        );
    }
}