using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class TicketDto : CreateTicketDto
{
    public int Id { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime PurchaseDateTimeUtc { get; set; }
    public bool IsReturned { get; set; } = false;
    public bool IsMissed { get; set; } = false;
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
    
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime PurchaseDateTimeUtc { get; set; }
    
    [Required]
    public bool IsReturned { get; set; } = false;
    
    [Required]
    public bool IsMissed { get; set; } = false;
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