using MediatR;

namespace Backend.Features.Post.Commands.DeletePost;

public record DeletePostCommand(Guid Id) : IRequest<ApiResult<Unit>>;