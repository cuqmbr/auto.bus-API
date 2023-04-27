namespace Server.Configurations;

public class SmtpCredentials
{
    public string Host { get; set; } = null!;
    public string Port { get; set; }
    public string User { get; set; } = null!;
    public string Password { get; set; } = null!;
}