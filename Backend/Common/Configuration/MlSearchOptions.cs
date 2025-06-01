using System.ComponentModel.DataAnnotations;

namespace Backend.Common.Configuration;

public class MlSearchOptions
{
    public const string SectionName = "MlSearch";

    [Required]
    public string BaseUrl { get; set; } = string.Empty;
}