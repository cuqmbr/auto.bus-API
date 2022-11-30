namespace SharedModels.QueryParameters.Objects;

public class RouteParameters : ParametersBase
{
    public const string DefaultFields = "id,type";
    
    public RouteParameters()
    {
        Fields = DefaultFields;
    }
    
    public string? Type { get; set; }
}