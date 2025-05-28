using Backend.Common.DbContext;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using Backend.Features.Post.Mappers;
using Backend.Features.Post.Queries.GetPostById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Post.Queries.GetPostsByUser;

public class
    GetPostByUserHandler : IRequestHandler<GetPostByUserQuery,
    ApiResult<InfiniteCursorPage<GetPostByIdResponse>>> // <<<>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;

    public GetPostByUserHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<InfiniteCursorPage<GetPostByIdResponse>>> Handle(GetPostByUserQuery request,
        CancellationToken cancellationToken)
    {
        var cursor = request.PaginationMeta.Cursor;
        var pageSize = request.PaginationMeta.PageSize;
        var currentUserId = _currentUserService.User!.UserId;
        var userExists =
            await _db.Users.AnyAsync(u => u.UserId == request.UserId, cancellationToken: cancellationToken);
        if (!userExists)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"User with id {request.UserId} not found.",
                Status = 404,
            });
        }

        var posts = await InfinitePaginationService.PaginateAsync(
            source: _db.Posts
                .Include(post => post.User)
                .Where(b => b.UserId == request.UserId)
                .OrderBy(b => b.CreatedAt)
                .ThenBy(b => b.PostId)
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
                }),
            keySelector: x => x.PostId,
            after: cursor,
            limit: pageSize
        );

        return ApiResult.Ok(posts);
    }
}