using MediatR;

namespace Backend.Features.Post.Queries;

public record GetPostByIdQuery(Guid Id) : IRequest<ApiResult<GetPostByIdResponse>>;