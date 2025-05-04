using MediatR;

namespace Backend.Features.Post.Commands;

public record DeletePostCommand(Guid Id) : IRequest<ApiResult<Unit>>;