using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects.Model;

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