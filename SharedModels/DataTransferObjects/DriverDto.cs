namespace SharedModels.DataTransferObjects;

public class DriverDto : UserDto
{
    public int CompanyId { get; set; }
}

public class CreateDriverDto : CreateUserDto
{
    public int CompanyId { get; set; }

    public override IList<string>? Roles { get; set; }
}

public class UpdateDriverDto : UpdateUserDto
{
    public override IList<string>? Roles { get; set; }
}