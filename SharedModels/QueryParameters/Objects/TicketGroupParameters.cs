namespace SharedModels.QueryParameters.Objects;

public class TicketGroupParameters : ParametersBase
{
    public const string DefaultFields = "id,purchaseDateTime,isReturned,userId";
    
    public TicketGroupParameters()
    {
        Fields = DefaultFields;
    }
    public DateTime? FromPurchaseDateTimeUtc { get; set; }
    public DateTime? ToPurchaseDateTimeUtc { get; set; }
    public bool? IsReturned { get; set; }
    public string? UserId { get; set; }
}