using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class RouteAddress
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("RouteId")]
    public int RouteId { get; set; }
    public Route Route { get; set; } = null!;
    
    [ForeignKey("AddressId")]
    public int AddressId { get; set; }
    public Address Address { get; set; } = null!;
    
    [ForeignKey("RouteAddressDetailsId")]
    public int RouteAddressDetailsId { get; set; }
    public virtual IList<RouteAddressDetails> RouteAddressDetails { get; set; } = null!;
    
    public int Order { get; set; }
}