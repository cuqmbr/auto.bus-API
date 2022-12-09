namespace SharedModels.QueryParameters.Objects;

public class TicketGroupParameters : ParametersBase
{
    public const string DefaultFields = "id,userId";
    
    public TicketGroupParameters()
    {
        Fields = DefaultFields;
    }
    
    public string? UserId { get; set; }
}