namespace SharedModels.QueryStringParameters;

public class RouteAddressParameters : QueryStringParameters
{
    public const string DefaultFields = "id,routeId,addressId,order,timeSpanToNextCity," +
                                        "waitTimeSpan,costToNextCity";
    
    public RouteAddressParameters()
    {
        Sort = "id";
        Fields = DefaultFields;
    }
    
    public int? RouteId { get; set; }
    public int? AddressId { get; set; }
}