namespace SharedModels.QueryParameters.Objects;

public class CompanyParameters : ParametersBase
{
    public const string DefaultFields = "id,ownerId,name,vehicles";
    
    public CompanyParameters()
    {
        Fields = DefaultFields;
    }

    public string? Name { get; set; }
    public string? OwnerId { get; set; }
}