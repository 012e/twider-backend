using Backend.Features.User.Queries;
using FluentResults;
using MediatR;

namespace Backend.Features.Post.Queries;

public class GetPostByIdHandler : IRequestHandler<GetPostByIdQuery, Result<GetPostByIdResponse>>
{
    public Task<Result<GetPostByIdResponse>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}