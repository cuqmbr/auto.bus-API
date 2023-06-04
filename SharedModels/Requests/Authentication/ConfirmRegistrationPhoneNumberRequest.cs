using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests.Authentication;

public class ConfirmRegistrationPhoneNumberRequest : SendConfirmationRegistrationPhoneNumberRequest
{
    [Required]
    public string Token { get; set; } = null!;
}