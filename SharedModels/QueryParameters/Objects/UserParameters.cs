namespace SharedModels.QueryParameters.Objects;

public class UserParameters : ParametersBase
{
    public const string DefaultFields = "";
    
    public UserParameters()
    {
        Sort = "id";
        Fields = DefaultFields;
    }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}