using Microsoft.AspNetCore.Identity;
using Server.Entities;

namespace Server.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = null!;
}