using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class RouteAddressDto : CreateRouteAddressDto
{
    public int Id { get; set; }

    public RouteDto Route { get; set; } = null!;

    public AddressDto Address { get; set; } = null!;
}

public class CreateRouteAddressDto
{
    [Required]
    public int RouteId { get; set; }
    
    [Required]
    public int AddressId { get; set; }
    
    [Required]
    public int Order { get; set; }
    
    [Required]
    [DataType(DataType.Duration)]
    public TimeSpan TimeSpanToNextCity { get; set; }
    
    [Required]
    [DataType(DataType.Duration)]
    public TimeSpan WaitTimeSpan { get; set; }
    
    [Required]
    [DataType(DataType.Currency)]
    public double CostToNextCity { get; set; }
}