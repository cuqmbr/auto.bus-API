namespace SharedModels.QueryStringParameters;

public class CountryParameters : QueryStringParameters
{
    public CountryParameters()
    {
        Sort = "id";
    }
    
    public string? Code { get; set; }
    public string? Name { get; set; }
}