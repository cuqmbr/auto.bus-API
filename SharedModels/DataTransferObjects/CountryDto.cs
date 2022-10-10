using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class CountryDto : CreateCountryDto
{
    public int Id { get; set; }
    
    public virtual IList<StateDto> States { get; set; } = null!;
}

public class CreateCountryDto
{
    [Required]
    [StringLength(maximumLength: 2, MinimumLength = 2, ErrorMessage = "The Code field must be 2 characters long")]
    public string Code { get; set; } = null!;
    
    [Required]
    [StringLength(maximumLength: 56, ErrorMessage = "The Name field must be shorter than 56 characters")]
    public string Name { get; set; } = null!;
}

public class UpdateCountryDto : CreateCountryDto
{
    [Required]
    public int Id { get; set; }
}