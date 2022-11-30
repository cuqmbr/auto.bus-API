using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class RouteAddressDetails
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("VehicleEnrollmentId")]
    public int VehicleEnrollmentId { get; set; }
    public VehicleEnrollment VehicleEnrollment { get; set; } = null!;
    
    [ForeignKey("RouteAddressId")]
    public int RouteAddressId { get; set; }
    public RouteAddress RouteAddress { get; set; } = null!;
    
    public TimeSpan TimeSpanToNextCity { get; set; }
    public TimeSpan WaitTimeSpan { get; set; }
    public double CostToNextCity { get; set; }
}