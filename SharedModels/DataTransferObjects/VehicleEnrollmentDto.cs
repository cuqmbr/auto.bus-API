using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class VehicleEnrollmentDto : CreateVehicleEnrollmentDto
{
    public int Id { get; set; }
}

public class CreateVehicleEnrollmentDto
{
    [Required]
    public int VehicleId { get; set; }
    
    [Required]
    public int RouteId { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime DepartureDateTimeUtc { get; set; }
}

public class UpdateVehicleEnrollmentDto : CreateVehicleEnrollmentDto
{
    [Required]
    public int Id { get; set; }
    
    public TimeSpan DelayTimeSpan { get; set; } = TimeSpan.Zero;
    
    public bool IsCanceled { get; set; } = false;
    public string CancelationComment { get; set; } = null!;
}

public class CreateVehicleEnrollmentWithDetailsDto : CreateVehicleEnrollmentDto
{
    public IList<CreateRouteAddressDetailsInVehicleEnrollmentDto> RouteAddressDetails { get; set; } = null!;
}

public class VehicleEnrollmentWithDetailsDto
{
    public int Id { get; set; }
    
    public IList<RouteAddressDetailsInVehicleEnrollmentDto> RouteAddressDetails { get; set; } = null!;
}