using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects.Model;

public class RouteDto : CreateRouteDto
{
    [Required]
    public int Id { get; set; }
    
    public new IList<RouteAddressDto> RouteAddresses { get; set; } = null!;
}

public class CreateRouteDto
{
    [Required]
    public string Type { get; set; } = null!;
    
    [Required]
    [MinLength(2)]
    public IList<CreateRouteAddressDto> RouteAddresses { get; set; } = null!;
}

public class UpdateRouteDto : RouteDto
{
    public new IList<RouteAddressDto> RouteAddresses { get; set; } = null!;
}