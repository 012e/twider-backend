using System.ComponentModel.DataAnnotations;

namespace Backend.Common.Configuration;

public class MinioOptions
{
    public const string SectionName = "Minio";

    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string AccessKey { get; set; } = string.Empty;

    [Required]
    public string SecretKey { get; set; } = string.Empty;
}