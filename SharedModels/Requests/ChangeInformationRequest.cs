using System.ComponentModel.DataAnnotations;
using Utils;

namespace SharedModels.Requests;

public class ChangeInformationRequest
{
    [Required]
    public string FistName { get; set; } = null!;
    [Required]
    public string LastName { get; set; } = null!;
    [Required]
    public string Patronymic { get; set; } = null!;
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }
    
    [Required]
    public Identity.Gender Gender { get; set; }
}