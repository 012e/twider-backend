using Backend.Common.DbContext;
using Backend.Common.Helpers.Types;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.User.Queries;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, ApiResult<GetUserByIdResponse>>
{
    private readonly ApplicationDbContext _db;

    public GetUserByIdHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResult<GetUserByIdResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Where(u => u.UserId == request.Id)
            .Select(u => new GetUserByIdResponse
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
            return ApiResult<GetUserByIdResponse>.Fail(new ProblemDetails
            {
                Title = "User not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"User with ID {request.Id} not found."
            });
        }

        return ApiResult<GetUserByIdResponse>.Ok(user);
    }
}