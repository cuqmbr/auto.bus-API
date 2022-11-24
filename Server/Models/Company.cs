using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class Company
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; } = null!;
    
    [ForeignKey("OwnerId")]
    public string OwnerId { get; set; }  = null!;
    public User Owner { get; set; } = null!;

    public virtual List<Vehicle> Vehicles { get; set; } = null!;
}