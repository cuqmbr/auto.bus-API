using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests.Account;

public class ChangeEmailRequest
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string NewEmail { get; set; } = null!;
}