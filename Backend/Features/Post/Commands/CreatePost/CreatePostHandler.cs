using Backend.Common.DbContext;
using Backend.Common.DbContext.Post;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Post.Commands.CreatePost;

public class CreatePostHandler : IRequestHandler<CreatePostCommand, ApiResult<ItemId>>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public CreatePostHandler(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<ItemId>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var user = _currentUserService.User;
        if (user == null)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "User not found",
                Detail = $"User with ID {user} not found."
            });
        }

        var media = await _db.UnknownMedia
            .Where(m => request.MediaIds.Contains(m.MediaId))
            .ToListAsync(cancellationToken);

        if (media.Count != request.MediaIds.Count)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Media not found",
                Detail = $"Media with IDs [{string.Join(", ", request.MediaIds)}] not found."
            });
        }

        _db.UnknownMedia.RemoveRange(media);
        var postMedia = media.AsQueryable().Select(m => new Common.DbContext.Post.PostMedium
        {
            MediaId = m.MediaId,
            MediaOwnerType = "post",
        }).ToList();

        var post = _db.Posts.Add(new Common.DbContext.Post.Post
        {
            Content = request.Content,
            UserId = user.UserId,
            Media = postMedia,
        });

        await _db.SaveChangesAsync(cancellationToken);

        return ApiResult.Ok(new ItemId(post.Entity.PostId));
    }
}