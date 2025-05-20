namespace Backend.Features.Media.Commands;

public class GenerateUploadUrlResponse
{
    public string Url { get; set; } = null!;
    public Guid MediumId { get; set; }
}