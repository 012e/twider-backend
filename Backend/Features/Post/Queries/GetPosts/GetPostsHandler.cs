using Backend.Common.DbContext;
using Backend.Common.Helpers;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using Backend.Features.Post.Mappers;
using Backend.Features.Post.Queries.GetPostById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Post.Queries;

public class GetPostsHandler : IRequestHandler<GetPostsQuery, ApiResult<InfiniteCursorPage<GetPostByIdResponse>>>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public GetPostsHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<InfiniteCursorPage<GetPostByIdResponse>>> Handle(GetPostsQuery request,
        CancellationToken cancellationToken)
    {
        var cursor = request.PaginationMeta.Cursor;
        var pageSize = request.PaginationMeta.PageSize;
        var currentUserId = _currentUserService.User!.UserId;

        var posts = await InfinitePaginationService.PaginateAsync(
            source: _db.Posts
                .Include(post => post.User)
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
                ),
            keySelector: x => x.PostId,
            after: cursor,
            limit: pageSize
        );

        return ApiResult.Ok(posts);
    }
}