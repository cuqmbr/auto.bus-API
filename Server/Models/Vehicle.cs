using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class Vehicle
{
    [Key]
    public int Id { get; set; }
    
    public string Number { get; set; } = null!;
    public string Type { get; set; } = null!;
    public int Capacity { get; set; }
    
    [ForeignKey("CompanyId")]
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    
    public bool HasClimateControl { get; set; }
    public bool HasWiFi { get; set; }
    public bool HasWC { get; set; }
    public bool HasStewardess { get; set; }
    public bool HasTV { get; set; }
    public bool HasOutlet { get; set; }
    public bool HasBelts { get; set; }
}