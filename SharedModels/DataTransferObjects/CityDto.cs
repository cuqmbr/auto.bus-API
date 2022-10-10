using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class CityDto : CreateCityDto
{
    public int Id { get; set; }
    
    public StateDto State { get; set; } = null!;
    
    public virtual IList<AddressDto>? Addresses { get; set; }
}

public class CreateCityDto
{
    [Required]
    [StringLength(maximumLength: 40, ErrorMessage = "City Name is too long")]
    public string Name { get; set; } = null!;
    
    [Required]
    public int StateId { get; set; }
}