using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests;

public class ConfirmChangeEmailRequest : ChangeEmailRequest
{
    [Required]
    public string Token { get; set; } = null!;
}