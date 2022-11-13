using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class VehicleEnrollmentDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int RouteId { get; set; }
    public DateTime DepartureDateTimeUtc { get; set; }
    public TimeSpan DelayTimeSpan { get; set; }
    public bool IsCanceled {get; set; }
    public string CancelationComment { get; set; } = null!; 
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
    
    public TimeSpan DelayTimeSpan { get; set; } = TimeSpan.Zero;

    public bool IsCanceled { get; set; } = false;
    public string? CancelationComment { get; set; } = null!;
}

public class UpdateVehicleEnrollmentDto
{
    [Required]
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int RouteId { get; set; }
    public DateTime DepartureDateTimeUtc { get; set; }
    public TimeSpan DelayTimeSpan { get; set; }
    public bool IsCanceled {get; set; }
    public string CancelationComment { get; set; } = null!;
}