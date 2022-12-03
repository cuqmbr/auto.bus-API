using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class RouteAddressDto : CreateRouteAddressDto
{
    public int Id { get; set; }
}

public class CreateRouteAddressDto
{
    [Required]
    public int RouteId { get; set; }
    
    [Required]
    public int AddressId { get; set; }
    
    [Required]
    [Range(0, Int32.MaxValue)]
    public int Order { get; set; }
}

public class UpdateRouteAddressDto : CreateRouteAddressDto
{
    [Required]
    public int Id { get; set; }
}

public class CreateRouteAddressWithAddressDto
{
    [Range(0, Int32.MaxValue)]
    public int Order { get; set; }
    
    public CreateAddressInRouteAddress? Address { get; set; }
    
    public int? AddressId { get; set; }
}

public class RouteAddressWithAddressDto
{
    public int Id { get; set; }
    
    public int Order { get; set; }
    
    public AddressInRouteAddress Address { get; set; } = null!;
    
    public int AddressId { get; set; }
}