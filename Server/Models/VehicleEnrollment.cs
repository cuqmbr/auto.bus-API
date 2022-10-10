using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class VehicleEnrollment
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("VehicleId")]
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    
    [ForeignKey("RouteId")]
    public int RouteId { get; set; }
    public Route Route { get; set; } = null!;
    
    public DateOnly DepartureDateOnly { get; set; }
    public TimeSpan DelayTimeSpan { get; set; }
}