using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class CompanyDto
{
    public int Id { get; set; }
    
    public string Name { get; set; } = null!;
    public string OwnerId { get; set; } = null!;
}

public class CreateCompanyDto
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string OwnerId { get; set; } = null!;
}

public class UpdateCompanyDto
{
    [Required]
    public int Id { get; set; }
    
    public string Name { get; set; } = null!;
    public string OwnerId { get; set; } = null!;
}