using System.ComponentModel.DataAnnotations;
using Backend.Common.Helpers.Types;
using MediatR;

namespace Backend.Features.User.Queries;

public class GetUserByIdQuery : IRequest<ApiResult<GetUserByIdResponse>>
{
    [Required]
    public Guid Id { get; set; }
}