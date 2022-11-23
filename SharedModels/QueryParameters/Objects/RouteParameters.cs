namespace SharedModels.QueryParameters.Objects;

public class RouteParameters : ParametersBase
{
    public const string DefaultFields = "id,type";
    
    public RouteParameters()
    {
        Sort = "id";
        Fields = DefaultFields;
    }
    
    public string? Type { get; set; }
}