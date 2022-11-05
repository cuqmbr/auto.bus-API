namespace SharedModels.QueryStringParameters;

public class CityParameters : QueryStringParameters
{
    public const string DefaultFields = "id,name,stateId";
    
    public CityParameters()
    {
        Sort = "id";
        Fields = DefaultFields;
    }
    
    public string? Name { get; set; }
    public int? StateId { get; set; }
}