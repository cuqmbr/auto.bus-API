using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class StateDto : CreateStateDto
{
    public int Id { get; set; }
    public CountryDto Country { get; set; } = null!;
    
    public virtual IList<CityDto> Cities { get; set; } = null!;
}

public class CreateStateDto
{
    [Required]
    [StringLength(maximumLength: 40, ErrorMessage = "State Name is too long")]
    public string Name { get; set; } = null!;
    
    [Required]
    public int CountryId { get; set; }
}