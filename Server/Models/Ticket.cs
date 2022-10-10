using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class Ticket
{
    [ForeignKey("UserId")]
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;
    
    [ForeignKey("VehicleEnrollmentId")]
    public int VehicleEnrollmentId { get; set; }
    public VehicleEnrollment VehicleEnrollment { get; set; } = null!;
    
    public DateTime PurchaseDateTimeUtc { get; set; }
    public bool IsReturned { get; set; } = false;
}