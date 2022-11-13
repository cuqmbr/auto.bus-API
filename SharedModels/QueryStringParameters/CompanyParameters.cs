namespace SharedModels.QueryStringParameters;

public class CompanyParameters : QueryStringParameters
{
    public const string DefaultFields = "id,ownerId,name";
    
    public CompanyParameters()
    {
        Sort = "id";
        Fields = DefaultFields;
    }

    public string? Name { get; set; }
    public string? OwnerId { get; set; }
}