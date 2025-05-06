using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Backend.Features.Post.Commands.UpdatePost;

public class UpdatePostCommand : IRequest<ApiResult<Unit>>
{
    [Required] public Guid PostId { get; set; }

    [Required] public UpdateContent Content { get; set; } = null!;

    public class UpdateContent
    {
        [Required] public string Content { get; set; } = null!;
    }
}
