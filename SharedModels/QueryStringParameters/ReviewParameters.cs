namespace SharedModels.QueryStringParameters;

public class ReviewParameters : QueryStringParameters
{
    public const string DefaultFields = "id,userId,vehicleEnrollmentId,rating,comment";
    
    public ReviewParameters()
    {
        Sort = "id";
        Fields = DefaultFields;
    }
    
    public int? Rating { get; set; }
    public string? Comment { get; set; }
}