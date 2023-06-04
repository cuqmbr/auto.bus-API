using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests.Authentication;

public class SendConfirmationRegistrationPhoneNumberRequest
{
    [Required]
    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; } = null!;
}