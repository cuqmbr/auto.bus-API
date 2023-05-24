using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class VehicleEnrollment
{
    [Key] public int Id { get; set; }

    [ForeignKey("VehicleId")] public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    [ForeignKey("RouteId")] public int RouteId { get; set; }
    public Route Route { get; set; } = null!;

    public virtual IList<RouteAddressDetails> RouteAddressDetails { get; set; } = null!;

    public DateTime DepartureDateTimeUtc { get; set; }

    public string? CancellationComment { get; set; } = null!;

    public IList<Ticket> Tickets { get; set; } = null!;
    public IList<Review> Reviews { get; set; } = null!;

    public bool IsCancelled() => CancellationComment == null;
}