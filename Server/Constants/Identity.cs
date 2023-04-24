namespace Server.Constants;

public class Identity
{
    public enum Roles
    {
        User,
        Driver,
        Company,
        Administrator
    }

    public const string DefaultUsername = "admin";
    public const string DefaultEmail = "admin@subdomain.domain";
    public const string DefaultPassword = "123qwe!@#QWE";
    public const Roles DefaultRole = Roles.Administrator;
}
