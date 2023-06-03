using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests;

public class ConfirmRegistrationPhoneNumberRequest
{
    [Required]
    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; } = null!;
    
    [Required]
    public string Token { get; set; } = null!;
}