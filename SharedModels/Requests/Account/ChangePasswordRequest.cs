using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests.Account;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = null!;
    
    [Required]
    public string NewPassword { get; set; } = null!;
}