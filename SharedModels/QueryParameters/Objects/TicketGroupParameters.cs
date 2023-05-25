namespace SharedModels.QueryParameters.Objects;

public class TicketGroupParameters : ParametersBase
{
    public const string DefaultFields =
        "id,userId,purchaseDateTime,isReturned,isPurchased,departureAddressName,departureCityName,departureStateName,departureCountryName," +
        "departureFullName,departureDateTime,arrivalAddressName,arrivalCityName,arrivalStateName,arrivalCountryName,arrivalFullName," +
        "arrivalDateTime,cost,tickets";
    
    public TicketGroupParameters()
    {
        Fields = DefaultFields;
    }
    public DateTime? FromPurchaseDateTime { get; set; }
    public DateTime? ToPurchaseDateTime { get; set; }
    public bool? IsReturned { get; set; }
    public string? UserId { get; set; }
}