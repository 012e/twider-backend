using Backend.Features.User.Queries;
using MediatR;

namespace Backend.Features.Post.Queries;

public class GetPostByIdHandler : IRequestHandler<GetPostByIdQuery, ApiResult<GetPostByIdResponse>>
{
    public Task<ApiResult<GetPostByIdResponse>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}