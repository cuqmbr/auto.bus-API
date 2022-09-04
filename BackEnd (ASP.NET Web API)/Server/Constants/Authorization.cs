namespace Server.Constants;

public class Authorization
{
    public enum Roles
    {
        Admin,
        User
    }

    public const string DefaultUsername = "user";
    public const string DefaultEmail = "user@email.com";
    public const string DefaultPassword = "125ASgl^%@lsdgjk!@#%^12eas";
    public const Roles DefaultRole = Roles.User;
}