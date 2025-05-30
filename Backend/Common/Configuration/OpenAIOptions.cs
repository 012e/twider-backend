using System.ComponentModel.DataAnnotations;

namespace Backend.Common.Configuration;

public class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    [Required] public string ApiKey { get; set; } = string.Empty;
}