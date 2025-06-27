using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Backend.Features.Media.Commands;

public class GenerateUploadUrlCommand : IRequest<ApiResult<GenerateUploadUrlResponse>>
{
    [Required]
    public string ContentType { get; set; } = null!;
}