using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class Ticket
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("TicketGroupId")]
    public int TicketGroupId { get; set; }
    public TicketGroup TicketGroup { get; set; } = null!;
    
    [ForeignKey("VehicleEnrollmentId")]
    public int VehicleEnrollmentId { get; set; }
    public VehicleEnrollment VehicleEnrollment { get; set; } = null!;
    
    public DateTime PurchaseDateTimeUtc { get; set; } =  DateTime.UtcNow;
    public int FirstRouteAddressId { get; set; }
    public int LastRouteAddressId { get; set; }
    public bool IsReturned { get; set; } = false;
    public bool IsMissed { get; set; } = false;
}