using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class Review
{
    [ForeignKey("UserId")]
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;
    
    [ForeignKey("VehicleEnrollmentId")]
    public int VehicleEnrollmentId { get; set; }
    public VehicleEnrollment VehicleEnrollment { get; set; } = null!;
    
    public int Rating { get; set; }
    public string Comment { get; set; } = null!;
}