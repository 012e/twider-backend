using Backend.Common.DbContext;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using Backend.Features.Post.Mappers;
using Backend.Features.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Post.Queries;

public class GetPostByIdHandler : IRequestHandler<GetPostByIdQuery, ApiResult<GetPostByIdResponse>>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public GetPostByIdHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<GetPostByIdResponse>> Handle(GetPostByIdQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.User!.UserId;
        var post = await _db.Posts
            .Include(p => p.User)
            .Select(b => new
            {
                PostId = b.PostId,
                Content = b.Content,
                CreatedAt = b.CreatedAt,
                User = b.User.ToUserDto(),
                UpdatedAt = b.UpdatedAt,
                Reactions = b.Reactions.ExtractReactionCount(),
                ReactionCount = b.Reactions.Count(),
                CommentCount = b.Comments.Count(),
                UserReaction = b.Reactions.FirstOrDefault(x => x.UserId == currentUserId)
            })
            .Select(b => new GetPostByIdResponse
                {
                    PostId = b.PostId,
                    Content = b.Content,
                    CreatedAt = b.CreatedAt,
                    User = b.User,
                    UpdatedAt = b.UpdatedAt,
                    Reactions = b.Reactions,
                    ReactionCount = b.ReactionCount,
                    CommentCount = b.CommentCount,
                    UserReaction = b.UserReaction == null ? null : b.UserReaction.ReactionType.ToFriendlyString()
                }
            )
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