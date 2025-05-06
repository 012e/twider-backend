using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Types;
using MediatR;

namespace Backend.Features.Post.Commands.CreatePost;

public class CreatePostCommand : IRequest<ApiResult<ItemId>>
{
    [Required]
    public string Content { get; set; } = null!;
}