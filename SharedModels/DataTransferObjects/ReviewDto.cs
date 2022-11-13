using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class ReviewDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = null!;
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

public class UpdateReviewDto
{
    [Required]
    public int Id { get; set; }
    
    public string UserId { get; set; } = null!;
    public int VehicleEnrollmentId { get; set; }
    
    [Range(0,100)]
    public int Rating { get; set; }
    
    [MaxLength(255)]
    public string Comment { get; set; } = null!;
}