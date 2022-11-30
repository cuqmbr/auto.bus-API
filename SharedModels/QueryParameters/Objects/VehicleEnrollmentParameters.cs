namespace SharedModels.QueryParameters.Objects;

public class VehicleEnrollmentParameters : ParametersBase
{
    public const string DefaultFields = "id,vehicleId,routeId,departureDateTimeUtc," +
                                        "delayTimeSpan,isCancelled,cancellationComment";
    
    public VehicleEnrollmentParameters()
    {
        Fields = DefaultFields;
    }
    
    public int? VehicleId { get; set; }
    public int? RouteId { get; set; }
    public DateTime? FromDepartureDateTime { get; set; }
    public DateTime? ToDepartureDateTime { get; set; }
    public bool? IsDelayed { get; set; }
    public bool? IsCanceled {get; set; }
}