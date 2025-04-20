using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Backend.Features.User.Queries;

public class GetUserByIdQuery : IRequest<ApiResult<GetUserByIdResponse>>
{
    [Required]
    [Key]
    [Range(0, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
    public int Id { get; set; }
}