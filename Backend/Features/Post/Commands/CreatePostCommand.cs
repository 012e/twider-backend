using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Backend.Features.Post.Commands;

public class CreatePostCommand : IRequest<ApiResult<CreatePostResponse>>
{
    [Required]
    public string Title { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;
}