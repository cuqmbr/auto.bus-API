namespace SharedModels.QueryParameters.Objects;

public class RouteParameters : ParametersBase
{
    public const string DefaultFields = "id,type,routeAddresses";
    
    public RouteParameters()
    {
        Fields = DefaultFields;
    }
    
    public string? Type { get; set; }
}