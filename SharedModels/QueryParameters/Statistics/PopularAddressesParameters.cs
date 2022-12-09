namespace SharedModels.QueryParameters.Statistics;

public class PopularAddressesParameters : ParametersBase
{
    public readonly string DefaultFields = "id,name,fullName,purchaseCount";
    public readonly int DefaultDays = 60;
    
    public PopularAddressesParameters() 
    {
        Fields = DefaultFields;
    }

    public int? Days { get; set; }
}