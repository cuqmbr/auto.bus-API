using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests.Authentication;

public class ConfirmRegistrationEmailRequest : SendConfirmationRegistrationEmailRequest
{
    [Required]
    public string Token { get; set; } = null!;
}