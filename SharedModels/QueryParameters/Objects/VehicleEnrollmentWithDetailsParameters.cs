namespace SharedModels.QueryParameters.Objects;

public class VehicleEnrollmentWithDetailsParameters : ParametersBase
{
    public const string DefaultFields = "id,vehicleId,routeId,departureDateTimeUtc," +
                                        "delayTimeSpan,isCancelled,cancellationComment," +
                                        "routeAddressDetails";
    
    public VehicleEnrollmentWithDetailsParameters()
    {
        Fields = DefaultFields;
    }
    
    public int? VehicleId { get; set; }
    public int? RouteId { get; set; }
    public DateTime? FromDepartureDateTime { get; set; }
    public DateTime? ToDepartureDateTime { get; set; }
    public bool? IsDelayed { get; set; }
    public bool? IsCanceled {get; set; }
    public TimeSpan? FromTotalTripDuration { get; set; }
    public TimeSpan? ToTotalTripDuration { get; set; }
    public double? FromCost { get; set; }
    public double? ToCost { get; set; }
}