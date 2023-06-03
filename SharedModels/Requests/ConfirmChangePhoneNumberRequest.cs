using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests;

public class ConfirmChangePhoneNumberRequest : ChangePhoneNumberRequest
{
    [Required]
    public string Token { get; set; } = null!;
}