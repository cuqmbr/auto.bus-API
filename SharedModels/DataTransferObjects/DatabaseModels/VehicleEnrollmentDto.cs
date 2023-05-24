using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SharedModels.DataTransferObjects.Model;

public class VehicleEnrollmentDto : CreateVehicleEnrollmentDto
{
    public int Id { get; set; }

    public string? CancellationComment { get; set; }
    
    [Required]
    public new IList<RouteAddressDetailsInVehicleEnrollmentDto> RouteAddressDetails { get; set; } = null!;
}

public class CreateVehicleEnrollmentDto
{
    [Required]
    public int VehicleId { get; set; }
    
    [Required]
    public int RouteId { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime DepartureDateTime { get; set; }
    
    [Required]
    public IList<CreateRouteAddressDetailsInVehicleEnrollmentDto> RouteAddressDetails { get; set; } = null!;
}

public class UpdateVehicleEnrollmentDto : VehicleEnrollmentDto { }

public class InReviewVehicleEnrollmentDto
{
    public int Id { get; set; }

    public int VehicleId { get; set; }
    public InVehicleEnrollmentVehicleDto Vehicle { get; set; } = null!;
    
    public int RouteId { get; set; }
    public RouteDto Route { get; set; } = null!;

    public DateTime DepartureDateTime { get; set; }
}