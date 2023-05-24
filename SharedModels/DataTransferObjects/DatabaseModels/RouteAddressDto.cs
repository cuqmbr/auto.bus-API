using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects.Model;

public class RouteAddressDto : CreateRouteAddressDto
{
    public int Id { get; set; }

    public string AddressName { get; set; } = null!;
    public string CityName { get; set; } = null!;
    public string StateName { get; set; } = null!;
    public string CountryName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class CreateRouteAddressDto
{
    [Required]
    public int AddressId { get; set; }
    
    [Required]
    [Range(0, Int32.MaxValue)]
    public int Order { get; set; }
}

public class UpdateRouteAddressDto : CreateRouteDto
{
    [Required]
    public int Id { get; set; }
}