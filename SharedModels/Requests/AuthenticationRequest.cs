using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests;

public class AuthenticationRequest
{
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}