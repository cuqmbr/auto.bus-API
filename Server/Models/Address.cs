using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public City City { get; set; } = null!;
    
    public virtual IList<RouteAddress> RouteAddresses { get; set; } = null!;
    
    public string GetFullName()
    {
        if (City == null || City.State == null || City.State.Country == null)
        {
            throw new NullReferenceException(
                $"Properties {nameof(City)}, {nameof(City.State)}, " +
                $"{nameof(City.State.Country)} must not be null");
        }
        
        return $"{City.GetFullName()}, {Name}";
    }
}
