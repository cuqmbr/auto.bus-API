using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests;

public class RegistrationRequest
{
    [Required(ErrorMessage = "Firstname is required")]
    public string FirstName { get; set; } = null!;
    
    [Required(ErrorMessage = "Lastname is required")]
    public string LastName { get; set; } = null!;
    
    [Required(ErrorMessage = "Patronymic is required")]
    public string Patronymic { get; set; } = null!;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone]
    public string PhoneNumber { get; set; } = null!;
    
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}
