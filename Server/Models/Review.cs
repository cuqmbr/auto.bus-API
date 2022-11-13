using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class Review
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("UserId")]
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;
    
    [ForeignKey("VehicleEnrollmentId")]
    public int VehicleEnrollmentId { get; set; }
    public VehicleEnrollment VehicleEnrollment { get; set; } = null!;
    
    public int Rating { get; set; }
    public string? Comment { get; set; }
}