using MediatR;

namespace Backend.Features.Post.Queries.GetPostById;

public record GetPostByIdQuery(Guid Id) : IRequest<ApiResult<GetPostByIdResponse>>;