using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Types;
using Backend.Features.Post.Queries.GetPostById;
using MediatR;

namespace Backend.Features.Post.Queries.GetPostsByUser;

public class GetPostByUserQuery : IRequest<ApiResult<InfiniteCursorPage<GetPostByIdResponse>>>
{
    [Required]
    public InfiniteCursorPaginationMeta PaginationMeta { get; set; } = new InfiniteCursorPaginationMeta();

    [Required]
    public Guid UserId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = new List<ValidationResult>();
        var validationContextForMeta = new ValidationContext(PaginationMeta);
        Validator.TryValidateObject(PaginationMeta, validationContextForMeta, validationResults, true);

        foreach (var result in validationResults)
        {
            yield return result;
        }
    }
}