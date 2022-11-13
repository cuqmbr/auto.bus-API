namespace SharedModels.QueryStringParameters;

public class RouteParameters : QueryStringParameters
{
    public const string DefaultFields = "id,type";
    
    public RouteParameters()
    {
        Sort = "id";
        Fields = DefaultFields;
    }
    
    public string? Type { get; set; }
}