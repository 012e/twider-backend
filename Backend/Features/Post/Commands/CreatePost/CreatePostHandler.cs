using Backend.Common.DbContext;
using Backend.Common.DbContext.Post;
using Backend.Common.Helpers.Types;
using Backend.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Backend.Features.Post.Commands.CreatePost;

public class CreatePostHandler : IRequestHandler<CreatePostCommand, ApiResult<ItemId>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _db;
    private readonly IPublicUrlGenerator _publicUrlGenerator;
    private readonly INatsJSContext _js;

    public CreatePostHandler(ApplicationDbContext db, ICurrentUserService currentUserService,
        IPublicUrlGenerator publicUrlGenerator, INatsJSContext js)
    {
        _db = db;
        _currentUserService = currentUserService;
        _publicUrlGenerator = publicUrlGenerator;
        _js = js;
    }

    private async Task PublicMessage(Common.DbContext.Post.Post post)
    {
        var ack = await _js.PublishAsync($"post.created.{post.PostId}", new
        {
            Id = post.PostId,
            Content = post.Content,
            MediaUrls = post.Media.Select(m => m.Url).ToList(),
        });
        ack.EnsureSuccess();
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
        List<Task<string>> mediaUrlsTask;
        try
        {
            mediaUrlsTask = media
                .Select(s => _publicUrlGenerator.GenerateUrlAsync("media", s.Path))
                .ToList();
            var mediaUrls = await Task.WhenAll(mediaUrlsTask);

            var postMedia = media.AsQueryable().Zip(mediaUrls, (medium, url) =>
                new PostMedium
                {
                    MediaId = medium.MediaId,
                    Url = url,
                    OwnerType = "post",
                }).ToList();

            var post = _db.Posts.Add(new Common.DbContext.Post.Post
            {
                Content = request.Content,
                UserId = user.UserId,
                Media = postMedia,
            });

            await _db.SaveChangesAsync(cancellationToken);
            await PublicMessage(post.Entity);

            return ApiResult.Ok(new ItemId(post.Entity.PostId));
        }
        catch (System.IO.FileNotFoundException ex)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Media file not found",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            return ApiResult.Fail(new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error generating media URLs",
                Detail = ex.Message
            });
        }
    }
}