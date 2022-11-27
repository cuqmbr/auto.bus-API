namespace SharedModels.QueryParameters.Statistics;

public class EngagedUserParameters : ParametersBase
{
    public readonly string DefaultFields = "id,firstName,lastName,username,email,phoneNumber,purchaseCount";
    public readonly int DefaultDays = 60;
    
    public EngagedUserParameters() 
    {
        Fields = DefaultFields;
    }

    public int? Days { get; set; }
}