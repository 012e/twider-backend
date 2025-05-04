using Backend.Common.DbContext;
using Backend.Features.Post.Mappers;
using Backend.Features.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Post.Queries;

public class GetPostByIdHandler : IRequestHandler<GetPostByIdQuery, ApiResult<GetPostByIdResponse>>
{
    private readonly ApplicationDbContext _db;

    public GetPostByIdHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResult<GetPostByIdResponse>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == request.Id, cancellationToken);
        if (post is null)
        {
            return ApiResult<GetPostByIdResponse>.Fail(new ProblemDetails
            {
                Title = "Post not found",
                Detail = $"Post with ID {request.Id} not found.",
                Status = StatusCodes.Status404NotFound
            });
        }
        return ApiResult.Ok(post.ToResponse());
    }
}