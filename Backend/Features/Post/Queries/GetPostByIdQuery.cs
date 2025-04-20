using FluentResults;
using MediatR;

namespace Backend.Features.Post.Queries;

public record GetPostByIdQuery(int Id) : IRequest<Result<GetPostByIdResponse>>;