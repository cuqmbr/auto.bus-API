namespace SharedModels.QueryStringParameters;

public class StateParameters : QueryStringParameters
{
    public const string DefaultFields = "id,name,countryId";
    
    public StateParameters()
    {
        Sort = "id";
        Fields = DefaultFields;
    }
    
    public string? Name { get; set; }
    public int? CountryId { get; set; }
}