using System.ComponentModel.DataAnnotations;

namespace SharedModels.Requests;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = null!;
    
    [Required]
    public string NewPassword { get; set; } = null!;
}