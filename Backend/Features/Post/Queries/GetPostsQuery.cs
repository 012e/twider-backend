using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Types;
using MediatR;

namespace Backend.Features.Post.Queries;

public class GetPostsQuery : IValidatableObject, IRequest<ApiResult<InfiniteCursorPage<GetPostByIdResponse>>>
{
    [Required]
    public InfiniteCursorPaginationMeta PaginationMeta { get; set; } = new InfiniteCursorPaginationMeta();

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