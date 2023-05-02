using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class CompanyDriver
{
    [ForeignKey("UserId")]
    public string DriverId { get; set; } = null!;
    public User Driver { get; set; } = null!;
    
    [ForeignKey("CompanyId")]
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}