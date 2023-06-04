using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests.Authentication;

public class AuthenticationRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    public string Password { get; set; } = null!;
}