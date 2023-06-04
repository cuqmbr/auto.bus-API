using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests.Authentication;

public class SendConfirmationRegistrationEmailRequest
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;
}