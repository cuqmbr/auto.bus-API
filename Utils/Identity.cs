namespace Utils;

public class Identity
{
    public enum Roles
    {
        User,
        Driver,
        Company,
        Administrator
    }

    public enum Gender
    {
        Male,
        Female
    }

    public enum Document
    {
        Passport,
        DriverLicence
    }

    public const string DefaultEmail = "admin@subdomain.domain";
    public const string DefaultPassword = "123qwe!@#QWE";
    public const Roles DefaultRole = Roles.User;
}