namespace SharedModels.QueryParameters.Objects;

public class TicketParameters : ParametersBase
{
    public const string DefaultFields = "id,userId,vehicleEnrollmentId,vehicleEnrollment," +
                                        "purchaseDateTimeUtc,isReturned,isMissed";
    
    public TicketParameters()
    {
        Fields = DefaultFields;
    }
    
    public DateTime? FromPurchaseDateTimeUtc { get; set; }
    public DateTime? ToPurchaseDateTimeUtc { get; set; }
    public bool? IsReturned { get; set; }
    public string? UserId { get; set; }
}