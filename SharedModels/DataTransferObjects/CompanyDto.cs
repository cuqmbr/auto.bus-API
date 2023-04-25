using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class CompanyDto : CreateCompanyDto
{
    public int Id { get; set; }

    public IList<VehicleDto> Vehicles { get; set; } = null!;
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

public class InVehicleCompanyDto
{
    public int Id { get; set; }
    
    public string Name { get; set; } = null!;
    public string OwnerId { get; set; } = null!;
}