using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Types;
using MediatR;

namespace Backend.Features.Search.Queries.SearchPosts;

public class SearchPostsQuery : IValidatableObject, IRequest<ApiResult<SearchPostsResponse>>
{
    [Required]
    [MinLength(1)]
    public string Query { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Offset { get; set; } = 0;

    [Range(1, 100)]
    public int Limit { get; set; } = 15;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            yield return new ValidationResult("Query cannot be empty or whitespace.", new[] { nameof(Query) });
        }
    }
}