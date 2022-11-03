namespace SharedModels.QueryStringParameters;

public class CountryParameters : QueryStringParameters
{
    public CountryParameters()
    {
        Sort = "id";
        Fields = "id,code,name,states";
    }
    
    public string? Code { get; set; }
    public string? Name { get; set; }
}