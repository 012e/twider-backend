using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Types;
using MediatR;

namespace Backend.Features.Comment.Commands.AddReaction;

public class AddReactionCommand : IRequest<ApiResult<Unit>>
{
    [Required] public Guid CommentId { get; set; }
    [Required] public Guid PostId { get; set; }

    public ReactionDto ReactionType { get; set; } = null!;

    public class ReactionDto
    {
        public ReactionType ReactionType { get; set; }
    }
}