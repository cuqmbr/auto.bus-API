using System.ComponentModel.DataAnnotations;
using SharedModels.Requests.Authentication;
using Utils;

namespace SharedModels.Requests;

public class DriverRegistrationRequest : RegistrationRequest
{
    public int? CompanyId { get; set; }
    
    [Required]
    public Identity.Gender Gender { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }
    
    [Required]
    public Identity.Document Document { get; set; }
    
    [Required]
    public string DocumentDetails { get; set; } = null!;
}