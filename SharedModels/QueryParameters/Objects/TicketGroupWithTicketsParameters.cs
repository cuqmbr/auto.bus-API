namespace SharedModels.QueryParameters.Objects;

public class TicketGroupWithTicketsParameters : ParametersBase
{
    public const string DefaultFields = "id,userId,tickets";
    
    public TicketGroupWithTicketsParameters()
    {
        Fields = DefaultFields;
    }
    
    public string? UserId { get; set; }
}