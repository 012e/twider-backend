using System.ComponentModel.DataAnnotations;

namespace Backend.Common.Configuration;

public class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";
    
    [Required]
    public string ApplicationDbContext { get; set; } = string.Empty;
}