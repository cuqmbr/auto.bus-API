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
}