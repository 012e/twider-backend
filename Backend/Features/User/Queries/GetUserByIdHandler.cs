using MediatR;

namespace Backend.Features.User.Queries;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, ApiResult<GetUserByIdResponse>>
{
    public async Task<ApiResult<GetUserByIdResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new GetUserByIdResponse
        {
            Id = request.Id,
        };

        return ApiResult.Ok(response);
    }
}