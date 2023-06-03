using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests;

public class ConfirmRegistrationEmailRequest
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    [Required]
    public string Token { get; set; } = null!;
}