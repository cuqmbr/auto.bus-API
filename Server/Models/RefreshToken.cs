using Microsoft.EntityFrameworkCore;

namespace Server.Models;

[Owned]
public class RefreshToken
{
    public string Token { get; set; } = null!;
    public DateTime ExpiryDateTime { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiryDateTime;
    public DateTime CreationDateTime { get; set; }
    public DateTime? Revoked { get; set; }
    public bool IsActive => Revoked == null && !IsExpired;
}