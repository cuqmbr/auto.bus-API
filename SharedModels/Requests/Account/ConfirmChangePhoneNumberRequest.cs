using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests.Account;

public class ConfirmChangePhoneNumberRequest : ChangePhoneNumberRequest
{
    [Required]
    public string Token { get; set; } = null!;
}