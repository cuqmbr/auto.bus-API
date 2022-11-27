using Microsoft.AspNetCore.Identity;

namespace Server.Models;

public class User : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public IList<RefreshToken> RefreshTokens { get; set; } = null!;
    public virtual IList<TicketGroup> TicketGroups { get; set; } = null!;
    public virtual IList<Review> Reviews { get; set; } = null!;
}