namespace SharedModels.QueryParameters.Objects;

public class CityParameters : ParametersBase
{
    public const string DefaultFields = "id,name,fullName,stateId,addresses";
    
    public CityParameters()
    {
        Fields = DefaultFields;
    }
    
    public string? Name { get; set; }
    public int? StateId { get; set; }
}