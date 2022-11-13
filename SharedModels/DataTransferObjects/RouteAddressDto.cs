using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class RouteAddressDto 
{
    public int Id { get; set; }
    public int RouteId { get; set; }
    public int AddressId { get; set; }
    public int Order { get; set; }
    [DataType(DataType.Duration)]
    public TimeSpan TimeSpanToNextCity { get; set; }
    [DataType(DataType.Duration)]
    public TimeSpan WaitTimeSpan { get; set; }
    [DataType(DataType.Currency)]
    public double CostToNextCity { get; set; }
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

public class UpdateRouteAddressDto
{
    [Required]
    public int Id { get; set; }
    public int RouteId { get; set; }
    public int AddressId { get; set; }
    public int Order { get; set; }
    [DataType(DataType.Duration)]
    public TimeSpan TimeSpanToNextCity { get; set; }
    [DataType(DataType.Duration)]
    public TimeSpan WaitTimeSpan { get; set; }
    [DataType(DataType.Currency)]
    public double CostToNextCity { get; set; }
}