using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects.Model;

public class TicketDto : CreateTicketDto
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    
    public InReviewVehicleEnrollmentDto VehicleEnrollment { get; set; } = null!;
}

public class CreateTicketDto
{
    [Required]
    public int TicketGroupId { get; set; }
    
    [Required]
    public int VehicleEnrollmentId { get; set; }
    
    [Required]
    public int FirstRouteAddressId { get; set; }
    
    [Required]
    public int LastRouteAddressId { get; set; }
}

public class UpdateTicketDto
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public int TicketGroupId { get; set; }
    
    [Required]
    public int VehicleEnrollmentId { get; set; }
}

public class CreateInTicketGroupTicketDto
{
    [Required]
    public int VehicleEnrollmentId { get; set; }
    
    [Required]
    public int FirstRouteAddressId { get; set; }
    
    [Required]
    public int LastRouteAddressId { get; set; }
}

public class InTicketGroupTicketDto : CreateInTicketGroupTicketDto
{
    public int Id { get; set; }
}

public class InVehicleEnrollmentTicketDto
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    
    public DateTime PurchaseDateTimeUtc { get; set; }
    
    public bool IsReturned { get; set; } = false;
    public bool IsMissed { get; set; } = false;
    
    public int TicketGroupId { get; set; }
    public int VehicleEnrollmentId { get; set; }
    
    public int FirstRouteAddressId { get; set; }
    public int LastRouteAddressId { get; set; }
}