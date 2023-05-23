using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects.Model;

public class RouteAddressDetailsDto : CreateRouteAddressDetailsDto
{
    public int Id { get; set; }
}

public class CreateRouteAddressDetailsDto
{
    [Required]
    public int VehicleEnrollmentId { get; set; }
    
    [Required]
    public int RouteAddressId { get; set; }

    [Required]
    public TimeSpan TimeSpanToNextCity { get; set; }
    
    [Required]
    public TimeSpan WaitTimeSpan { get; set; }
    
    [Required]
    public double CostToNextCity { get; set; }
}

public class UpdateRouteAddressDetailsDto : CreateRouteAddressDetailsDto
{
    [Required]
    public int Id { get; set; }
}

public class CreateRouteAddressDetailsInVehicleEnrollmentDto
{
    [Required]
    public int RouteAddressId { get; set; }

    [Required]
    public TimeSpan TimeSpanToNextCity { get; set; }
    
    [Required]
    public TimeSpan WaitTimeSpan { get; set; }
    
    [Required]
    public double CostToNextCity { get; set; }
}

public class RouteAddressDetailsInVehicleEnrollmentDto : CreateRouteAddressDetailsInVehicleEnrollmentDto
{
    public int Id { get; set; }
}