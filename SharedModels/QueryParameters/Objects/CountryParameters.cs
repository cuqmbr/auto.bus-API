namespace SharedModels.QueryParameters.Objects;

public class CountryParameters : ParametersBase
{
    public const string DefaultFields = "id,code,name";
    
    public CountryParameters()
    {
        Fields = DefaultFields;
    }
    
    public string? Code { get; set; }
    public string? Name { get; set; }
}