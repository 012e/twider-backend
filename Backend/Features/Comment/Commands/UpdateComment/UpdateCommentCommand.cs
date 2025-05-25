using System.ComponentModel.DataAnnotations;
using Backend.Features.Comment.Commands.CreateComment;
using MediatR;

namespace Backend.Features.Comment.Commands.UpdateComment;

public class UpdateCommentCommand : IRequest<ApiResult<Unit>>, IValidatableObject
{
    [Required]
    public Guid CommentId { get; set; }

    [Required]
    public Guid PostId { get; set; }

    [Required]
    public CommentContent Content { get; set; } = null!;


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(Content, validationContext, results, true);
        return results;
    }
}