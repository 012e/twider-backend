using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Types;
using MediatR;

namespace Backend.Features.Post.Commands;

public class PostReactionCommand : IRequest<ApiResult<Unit>>
{
    [Required] public Guid PostId { get; set; }

    public ReactionDto ReactionType { get; set; } = null!;

    public class ReactionDto
    {
        public ReactionType ReactionType { get; set; }
    }
}