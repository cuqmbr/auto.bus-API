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
    
    public DateTime DepartureDateTimeUtc { get; set; }

    public TimeSpan? DelayTimeSpan { get; set; }

    public bool IsCanceled { get; set; } = false;
    public string? CancelationComment { get; set; } = null!;
    
    public IList<Ticket> Tickets { get; set; } = null!;
    public IList<Review> Reviews { get; set; } = null!;
}