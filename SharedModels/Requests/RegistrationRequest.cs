using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests;

public class RegistrationRequest
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = null!;
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;
}