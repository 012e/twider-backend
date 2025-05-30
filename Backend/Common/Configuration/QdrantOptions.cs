using System.ComponentModel.DataAnnotations;

namespace Backend.Common.Configuration;

public class QdrantOptions
{
    public const string SectionName = "Qdrant";

    [Required] public string Url { get; set; } = string.Empty;

    public string? ApiKey { get; set; }
}