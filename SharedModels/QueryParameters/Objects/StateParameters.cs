namespace SharedModels.QueryParameters.Objects;

public class StateParameters : ParametersBase
{
    public const string DefaultFields = "id,name,fullName,cities,countryId,country";
    
    public StateParameters()
    {
        Fields = DefaultFields;
    }
    
    public string? Name { get; set; }
    public int? CountryId { get; set; }
}