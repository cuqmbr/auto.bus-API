using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedModels.DataTransferObjects;

namespace Server.Models;

public class State
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; } = null!;

    public virtual IList<City> Cities { get; set; } = null!;
    
    [ForeignKey("CountryId")]
    public int CountryId { get; set; }
    public Country Country { get; set; } = null!;
    
    public string GetFullName()
    {
        if (Country == null)
        {
            throw new NullReferenceException($"Property {nameof(Country)} must not be null");
        }
        
        return $"{Country.GetFullName()}, {Name}";
    }
}