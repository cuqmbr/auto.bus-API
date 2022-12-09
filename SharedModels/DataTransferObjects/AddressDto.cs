using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class AddressDto : CreateAddressDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
}

public class CreateAddressDto
{
    [Required]
    [StringLength(maximumLength: 250, ErrorMessage = "Address name is too long")]
    public string Name { get; set; } = null!;
    
    [Range(-90, 90, ErrorMessage = "Latitude must be in range(-90, 90)")]
    public double Latitude { get; set; }
    
    [Range(-180, 180, ErrorMessage = "Longitude must be in range(-180, 180)")]
    public double Longitude { get; set; }
    
    [Required]
    public int CityId { get; set; }
}

public class UpdateAddressDto : CreateAddressDto
{
    [Required]
    public int Id { get; set; }
}

public class InCityAddressDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class CreateAddressInRouteAddress
{
    [StringLength(maximumLength: 250, ErrorMessage = "Address name is too long")]
    public string Name { get; set; } = null!;
    
    [Range(-90, 90, ErrorMessage = "Latitude must be in range(-90, 90)")]
    public double Latitude { get; set; }
    
    [Range(-180, 180, ErrorMessage = "Longitude must be in range(-180, 180)")]
    public double Longitude { get; set; }
    
    public int CityId { get; set; }
}

public class AddressInRouteAddress : CreateAddressInRouteAddress
{
    public string FullName = null!;
}