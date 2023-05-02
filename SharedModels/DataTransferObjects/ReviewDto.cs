using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class ReviewDto : CreateReviewDto
{
    public int Id { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime PostDateTimeUtc { get; set; }

    public StrippedUserDto User { get; set; } = null!;
    public InReviewVehicleEnrollmentDto VehicleEnrollment { get; set; } = null!;
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
    public DateTime PostDateTime { get; set; }
}

public class InVehicleEnrollmentReviewDto
{
    public int Id { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime PostDateTimeUtc { get; set; }

    public string UserId { get; set; } = null!;
    public int VehicleEnrollmentId { get; set; }
    
    public int Rating { get; set; }
    public string Comment { get; set; } = null!;
}