using Backend.Common.DbContext;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using Backend.Features.Post.Mappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio;

namespace Backend.Features.Post.Queries.GetPostById;

public class GetPostByIdHandler : IRequestHandler<GetPostByIdQuery, ApiResult<GetPostByIdResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;
    private readonly IMinioClient _minioClient;

    public GetPostByIdHandler(ApplicationDbContext db, ICurrentUserService currentUserService, IMinioClient minioClient)
    {
        _db = db;
        _currentUserService = currentUserService;
        _minioClient = minioClient;
    }

    public async Task<ApiResult<GetPostByIdResponse>> Handle(GetPostByIdQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.User!.UserId;
        var post = await _db.Posts
            .Include(p => p.User)
            .Include(p => p.Media)
            .Select(b => new
            {
                Post = b,
                UserReaction = b.Reactions.FirstOrDefault(x => x.UserId == currentUserId)
            })
            .Select(b => new GetPostByIdResponse
            {
                PostId = b.Post.PostId,
                Content = b.Post.Content,
                CreatedAt = b.Post.CreatedAt,
                User = b.Post.User.ToUserDto(),
                UpdatedAt = b.Post.UpdatedAt,
                Reactions = b.Post.Reactions.ExtractReactionCount(),
                ReactionCount = b.Post.Reactions.Count(),
                CommentCount = b.Post.Comments.Count(),
                UserReaction = b.UserReaction == null ? null : b.UserReaction.ReactionType.ToFriendlyString(),
                MediaUrls = b.Post.Media.Select(r => r.Url).ToList(),
            })
            .FirstOrDefaultAsync(p => p.PostId == request.Id, cancellationToken);
        if (post is null)
        {
            return ApiResult<GetPostByIdResponse>.Fail(new ProblemDetails
            {
                Title = "Post not found",
                Detail = $"Post with ID {request.Id} not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return ApiResult.Ok(post);
    }
}