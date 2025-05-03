using System.ComponentModel.DataAnnotations;

namespace Backend.Common.Configuration;

public class KeycloakOptions
{
    public const string SectionName = "Keycloak";
    
    [Required]
    public string Url { get; set; } = string.Empty;
    
    [Required]
    public string Realm { get; set; } = string.Empty;
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}