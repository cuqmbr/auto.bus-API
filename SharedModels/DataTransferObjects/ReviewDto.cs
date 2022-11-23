using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class ReviewDto : CreateReviewDto
{
    public int Id { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime PostDateTimeUtc { get; set; }
}

public class CreateReviewDto
{
    [Required]
    public string UserId { get; set; } = null!;
    
    [Required]
    public int VehicleEnrollmentId { get; set; }
    
    [Range(0,100)]
    public int Rating { get; set; }
    
    [MaxLength(255)]
    public string Comment { get; set; } = null!;
}

public class UpdateReviewDto : CreateReviewDto
{
    [Required]
    public int Id { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime PostDateTimeUtc { get; set; }
}