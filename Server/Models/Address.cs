using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedModels.DataTransferObjects;

namespace Server.Models;

public class Address
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    [ForeignKey("CityId")]
    public int CityId { get; set; }
    public City? City { get; set; }
    
    public virtual IList<RouteAddress> RouteAddresses { get; set; } = null!;
    
    public override string ToString()
    {
        return $"{City.State.Country.Name}, {City.State.Name}, " +
               $"{City.Name}, {this.Name}";
    }
}