namespace SharedModels.QueryParameters.Objects;

public class ReviewParameters : ParametersBase
{
    public const string DefaultFields = "id,rating,comment,userId,user,vehicleEnrollmentId,vehicleEnrollment";
    
    public ReviewParameters()
    {
        Fields = DefaultFields;
    }
    
    public int? FromRating { get; set; }
    public int? ToRating { get; set; }

    public string? Comment { get; set; }
    public string? UserId { get; set; }
    public int? CompanyId { get; set; }
}