using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class RouteDto : CreateRouteDto
{
    public int Id { get; set; }

    public virtual IList<RouteAddressDto> RouteAddresses { get; set; } = null!;
}

public class CreateRouteDto
{
    [Required]
    public string Type { get; set; } = null!;
    
    [Required]
    public TimeOnly IntendedDepartureTimeOnlyUtc { get; set; }
}