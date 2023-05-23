using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects.Model;

public class CityDto : CreateCityDto
{
    public int Id { get; set; }
    
    public string FullName { get; set; } = null!;
    
    public virtual IList<InCityAddressDto>? Addresses { get; set; }
}

public class CreateCityDto
{
    [Required]
    [StringLength(maximumLength: 40, ErrorMessage = "City Name is too long")]
    public string Name { get; set; } = null!;
    
    [Required]
    public int StateId { get; set; }
}

public class UpdateCityDto : CreateCityDto
{
    [Required]
    public int Id { get; set; }
}

public class InStateCityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class InAddressCityDto
{
    public string Name { get; set; } = null!;
}