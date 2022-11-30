using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedModels.DataTransferObjects;

namespace Server.Models;

public class City
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; } = null!;

    public virtual IList<Address>? Addresses { get; set; }
    
    [ForeignKey("StateId")]
    public int StateId { get; set; }

    public State State { get; set; } = null!;
    
    public string GetFullName()
    {
        if (State == null || State.Country == null)
        {
            throw new NullReferenceException(
                $"Properties {nameof(State)}, " +
                $"{nameof(State.Country)} must not be null");
        }
        
        return $"{State.GetFullName()}, {Name}";
    }
}