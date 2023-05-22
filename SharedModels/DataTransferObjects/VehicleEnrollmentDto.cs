using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class VehicleEnrollmentDto : CreateVehicleEnrollmentDto
{
    public int Id { get; set; }

    public IList<InVehicleEnrollmentTicketDto> Tickets { get; set; } = null!;
    public IList<InVehicleEnrollmentReviewDto> Reviews { get; set; } = null!;
    
    public bool IsCanceled { get; set; }
    public string? CancelationComment { get; set; }
}

public class CreateVehicleEnrollmentDto
{
    [Required]
    public int VehicleId { get; set; }
    public VehicleDto? Vehicle { get; set; } = null!;
    
    [Required]
    public int RouteId { get; set; }
    public RouteDto? Route { get; set; } = null!;

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime DepartureDateTime { get; set; }
}

public class UpdateVehicleEnrollmentDto : CreateVehicleEnrollmentDto
{
    [Required]
    public int Id { get; set; }
    
    public TimeSpan DelayTimeSpan { get; set; } = TimeSpan.Zero;

    public bool? IsCanceled { get; set; } = false;
    public string? CancelationComment { get; set; }
}

public class CreateVehicleEnrollmentWithDetailsDto : CreateVehicleEnrollmentDto
{
    public IList<CreateRouteAddressDetailsInVehicleEnrollmentDto> RouteAddressDetails { get; set; } = null!;
}

public class VehicleEnrollmentWithDetailsDto : VehicleEnrollmentDto
{
    public IList<RouteAddressDetailsInVehicleEnrollmentDto> RouteAddressDetails { get; set; } = null!;
}

public class InReviewVehicleEnrollmentDto
{
    public int Id { get; set; }

    public int VehicleId { get; set; }
    public InVehicleEnrollmentVehicleDto Vehicle { get; set; } = null!;
    
    public int RouteId { get; set; }
    public RouteDto Route { get; set; } = null!;

    public DateTime DepartureDateTime { get; set; }
}