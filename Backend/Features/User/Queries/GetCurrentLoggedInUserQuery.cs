using MediatR;

namespace Backend.Features.User.Queries;

public class GetCurrentLoggedInUserQuery : IRequest<ApiResult<GetCurrentLoggedInUserResponse>>
{
}