using System.ComponentModel.DataAnnotations;
using SharedModels.DataTransferObjects;

namespace Server.Models;

public class Route
{
    [Key]
    public int Id { get; set; }
    
    public string Type { get; set; } = null!;

    public virtual IList<RouteAddress> RouteAddresses { get; set; } = null!;
    public virtual IList<VehicleEnrollment> VehicleEnrollments { get; set; } = null!;
}