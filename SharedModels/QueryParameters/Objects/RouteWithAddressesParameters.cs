namespace SharedModels.QueryParameters.Objects;

public class RouteWithAddressesParameters : ParametersBase
{
    public const string DefaultFields = "id,type,routeAddresses";
    
    public RouteWithAddressesParameters()
    {
        Fields = DefaultFields;
    }
    
    public string? Type { get; set; }
    public string? FromAddressName { get; set; }
    public string? ToAddressName { get; set; }
}