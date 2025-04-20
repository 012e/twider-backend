using System.ComponentModel.DataAnnotations;

namespace Backend.Features.User.Queries;

public class GetUserByIdResponse
{
    [Required]
    [Key]
    [Range(0, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
    public int Id { get; set; }
}