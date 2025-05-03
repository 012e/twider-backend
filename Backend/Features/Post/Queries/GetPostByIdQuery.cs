using MediatR;

namespace Backend.Features.Post.Queries;

public record GetPostByIdQuery(int Id) : IRequest<ApiResult<GetPostByIdResponse>>;