using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Types;
using Backend.Features.Post.Queries.GetPostById;
using MediatR;

namespace Backend.Features.Search.Queries.SearchPosts;

public class SearchPostsQuery : IValidatableObject, IRequest<ApiResult<InfiniteCursorPage<GetPostByIdResponse>>>
{
    [Required] [MinLength(1)] public string Query { get; set; } = string.Empty;

    [Required] public InfiniteCursorPaginationMeta PaginationMeta { get; set; } = new InfiniteCursorPaginationMeta();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            yield return new ValidationResult("Query cannot be empty or whitespace.", new[] { nameof(Query) });
        }

        var validationResults = new List<ValidationResult>();
        var validationContextForMeta = new ValidationContext(PaginationMeta);
        Validator.TryValidateObject(PaginationMeta, validationContextForMeta, validationResults, true);

        foreach (var result in validationResults)
        {
            yield return result;
        }
    }
}