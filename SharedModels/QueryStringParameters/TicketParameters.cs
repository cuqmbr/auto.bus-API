namespace SharedModels.QueryStringParameters;

public class TicketParameters : QueryStringParameters
{
    public const string DefaultFields = "id,userId,vehicleEnrollmentId,purchaseDateTimeUtc,isReturned";
    
    public TicketParameters()
    {
        Sort = "id";
        Fields = DefaultFields;
    }
    
    public DateTime? FromPurchaseDateTimeUtc { get; set; }
    public DateTime? ToPurchaseDateTimeUtc { get; set; }
    public bool? IsReturned { get; set; }
}