using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class CompanyDto : CreateCompanyDto
{
    public int Id { get; set; }
}

public class CreateCompanyDto
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string OwnerId { get; set; } = null!;
}

public class UpdateCompanyDto : CreateCompanyDto
{
    [Required]
    public int Id { get; set; }
}