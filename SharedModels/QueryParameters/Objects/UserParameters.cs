namespace SharedModels.QueryParameters.Objects;

public class UserParameters : ParametersBase
{
    public const string DefaultFields = "id,firstName,lastName,patronymic,email,emailConfirmed,phoneNumber," +
                                         "phoneNumberConfirmed,birthDate,gender,document,documentDetails," +
                                         "roles,reviews,ticketGroups";
    
    public UserParameters()
    {
        Sort = "";
        Fields = DefaultFields;
    }
}