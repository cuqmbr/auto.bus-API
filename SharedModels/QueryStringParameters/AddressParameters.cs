namespace SharedModels.QueryStringParameters;

public class AddressParameters : QueryStringParameters
{
    public const string DefaultFields = "id,name,cityId";
    
    public AddressParameters()
    {
        Sort = "id";
        Fields = DefaultFields;
    }
    
    public string? Name { get; set; }
    public int? CityId { get; set; }
}