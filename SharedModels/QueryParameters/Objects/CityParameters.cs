namespace SharedModels.QueryParameters.Objects;

public class CityParameters : ParametersBase
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