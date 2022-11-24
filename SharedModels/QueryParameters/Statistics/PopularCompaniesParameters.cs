namespace SharedModels.QueryParameters.Statistics;

public class PopularCompanyParameters : ParametersBase
{
    public readonly string DefaultFields = "id,ownerId,name,rating";
    
    public PopularCompanyParameters() 
    {
        Fields = DefaultFields;
    }
}