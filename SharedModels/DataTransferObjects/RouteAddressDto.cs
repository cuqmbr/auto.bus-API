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

public class UpdateRouteAddressDto : CreateRouteAddressDto
{
    [Required]
    public int Id { get; set; }
}

public class CreateRouteAddressWithAddressDto
{
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

    [Required] 
    public CreateAddressDto Address { get; set; } = null!;
}