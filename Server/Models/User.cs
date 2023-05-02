using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Utils;

namespace Server.Models;

public class User : IdentityUser
{
    [Required(ErrorMessage = "First Name is required")]
    public string FirstName { get; set; } = null!;
    
    [Required(ErrorMessage = "Last Name is required")]
    public string LastName { get; set; } = null!;
    
    [Required(ErrorMessage = "Patronymic is required")]
    public string Patronymic { get; set; } = null!;
    
    public DateTime? BirthDate { get; set; }
    
    public Identity.Gender? Gender { get; set; }
    
    public Identity.Document? Document { get; set; }
    
    public string? DocumentDetails { get; set; }

    public IList<RefreshToken> RefreshTokens { get; set; } = null!;

    public Company Company { get; set; } = null!;
    
    public virtual IList<TicketGroup> TicketGroups { get; set; } = null!;
    public virtual IList<Review> Reviews { get; set; } = null!;
    public virtual CompanyDriver? Employer { get; set; } = null!;

    public string GetFullName() => $"{LastName} {FirstName} {Patronymic}";
}