namespace SharedModels.QueryParameters.Objects;

public class RouteAddressParameters : ParametersBase
{
    public const string DefaultFields = "id,routeId,route,addressId,address,order" +
                                        ",timeSpanToNextCity,waitTimeSpan,costToNextCity";
    
    public RouteAddressParameters()
    {
        Fields = DefaultFields;
    }
    
    public int? RouteId { get; set; }
    public int? AddressId { get; set; }
}