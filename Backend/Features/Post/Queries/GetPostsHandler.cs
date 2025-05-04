using Backend.Common.DbContext;
using Backend.Common.Helpers;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using Backend.Features.Post.Mappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Post.Queries;

public class GetPostsHandler : IRequestHandler<GetPostsQuery, ApiResult<InfiniteCursorPage<GetPostByIdResponse>>>
{
    private readonly ApplicationDbContext _db;

    public GetPostsHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResult<InfiniteCursorPage<GetPostByIdResponse>>> Handle(GetPostsQuery request,
        CancellationToken cancellationToken)
    {
        var cursor = request.PaginationMeta.Cursor;
        var pageSize = request.PaginationMeta.PageSize;

        var posts = await InfinitePaginationService.PaginateAsync(
            source: _db.Posts.Include(post => post.User),
            keySelector: x => x.PostId,
            after: cursor,
            mapper: i => i.ToResponse(),
            limit: pageSize
        );

        return ApiResult.Ok(posts);
    }
}