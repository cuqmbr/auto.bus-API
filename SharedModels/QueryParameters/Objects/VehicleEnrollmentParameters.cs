namespace SharedModels.QueryParameters.Objects;

public class VehicleEnrollmentParameters : ParametersBase
{
    public const string DefaultFields = "id,vehicleId,vehicle,routeId,route,departureDateTime,tickets,reviews," +
                                        "isCancelled,cancellationComment,routeAddressDetails";
    
    public VehicleEnrollmentParameters()
    {
        Fields = DefaultFields;
    }
    
    public int? VehicleId { get; set; }
    public int? RouteId { get; set; }
    public DateTime? FromDepartureDateTime { get; set; }
    public DateTime? ToDepartureDateTime { get; set; }
    public bool? IsCancelled {get; set; }
    public TimeSpan? FromTotalTripDuration { get; set; }
    public TimeSpan? ToTotalTripDuration { get; set; }
    public double? FromCost { get; set; }
    public double? ToCost { get; set; }
}