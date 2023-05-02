namespace SharedModels.QueryParameters.Objects;

public class CompanyDriverParameters : ParametersBase
{
    public const string DefaultFields = "id,firstName,lastName,patronymic,email,emailConfirmed,phoneNumber," +
                                        "phoneNumberConfirmed,birthDate,gender,document,documentDetails," +
                                        "companyId";
    
    public CompanyDriverParameters()
    {
        Sort = "";
        Fields = DefaultFields;
    }
    
    public int? CompanyId { get; set; }
}