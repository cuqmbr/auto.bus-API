using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class VehicleDto
{
    public int Id { get; set; }
    public string? Number { get; set; }
    public string? Type { get; set; }
    public int Capacity { get; set; }
    public bool HasClimateControl { get; set; }
    public bool HasWiFi { get; set; }
    public bool HasWC { get; set; }
    public bool HasStewardess { get; set; }
    public bool HasTV { get; set; }
    public bool HasOutlet { get; set; }
    public bool HasBelts { get; set; }
    public int CompanyId { get; set; }
}

public class CreateVehicleDto
{
    [Required]
    [MaxLength(8)]
    public string? Number { get; set; }
    
    [Required]
    public string? Type { get; set; }
    
    [Required]
    [Range(10, 100)]
    public int Capacity { get; set; }

    [Required] 
    public bool HasClimateControl { get; set; } = false;
    
    [Required]
    public bool HasWiFi { get; set; } = false;
    
    [Required]
    public bool HasWC { get; set; } = false;
    
    [Required]
    public bool HasStewardess { get; set; } = false;
    
    [Required]
    public bool HasTV { get; set; } = false;
    
    [Required]
    public bool HasOutlet { get; set; } = false;
    
    [Required]
    public bool HasBelts { get; set; } = false;
    
    [Required]
    public int CompanyId { get; set; }
}

public class UpdateVehicleDto
{
    [Required]
    public int Id { get; set; }
    
    [MaxLength(8)]
    public string? Number { get; set; }
    public string? Type { get; set; }

    [Range(10, 100)]
    public int Capacity { get; set; }
    public bool HasClimateControl { get; set; } = false;
    public bool HasWiFi { get; set; } = false;
    public bool HasWC { get; set; } = false;
    public bool HasStewardess { get; set; } = false;
    public bool HasTV { get; set; } = false;
    public bool HasOutlet { get; set; } = false;
    public bool HasBelts { get; set; } = false;
    public int CompanyId { get; set; }
}