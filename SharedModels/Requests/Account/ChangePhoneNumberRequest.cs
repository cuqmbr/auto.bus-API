using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests.Account;

public class ChangePhoneNumberRequest
{
    [Required]
    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; } = null!;
}