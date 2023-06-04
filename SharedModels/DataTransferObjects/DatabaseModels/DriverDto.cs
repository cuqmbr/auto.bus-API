using System.ComponentModel.DataAnnotations;
using Utils;

namespace SharedModels.DataTransferObjects.Model;

public class DriverDto : UpdateDriverDto
{
    public string FullName { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
}

public class UpdateDriverDto
{
    [Required]
    public string Id { get; set; } = null!;
    
    [Required]
    public string FirstName { get; set; } = null!;
    [Required]
    public string LastName { get; set; } = null!;
    [Required]
    public string Patronymic { get; set; } = null!;
    
    [Required]
    public Identity.Gender Gender { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }
    
    [Required]
    public Identity.Document Document { get; set; }
    [Required]
    public string DocumentDetails { get; set; } = null!;
    
    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string PhoneNumber { get; set; } = null!;
    
    [Required]
    public int CompanyId { get; set; }
}