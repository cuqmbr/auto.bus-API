namespace Server.Settings;

public class Jwt
{
    public string Key { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public double ValidityInMinutes { get; set; }
}