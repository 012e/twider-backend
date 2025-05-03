using System.ComponentModel.DataAnnotations;

namespace Backend.Common.Configuration;

public class OAuthOptions
{
    public const string SectionName = "OAuth";
    
    [Required]
    public string ClientId { get; set; } = string.Empty;
    
    [Required]
    public string Audience { get; set; } = string.Empty;
    
    [Required]
    public string AuthorizationUrl { get; set; } = string.Empty;
    
    [Required]
    public string Authority { get; set; } = string.Empty;
    
    [Required]
    public string TokenUrl { get; set; } = string.Empty;
}