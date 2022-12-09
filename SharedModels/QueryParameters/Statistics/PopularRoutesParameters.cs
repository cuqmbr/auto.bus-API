namespace SharedModels.QueryParameters.Statistics;

public class PopularRoutesParameters : ParametersBase
{
    public readonly string DefaultFields = "departureAddressId,departureAddress," +
                                           "arrivalAddressId,arrivalAddress," +
                                           "count";
    public readonly int DefaultDays = 60;
    
    public PopularRoutesParameters() 
    {
        Fields = DefaultFields;
    }

    public int? Days { get; set; }
}