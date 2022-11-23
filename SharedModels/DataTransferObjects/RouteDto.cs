using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class RouteDto : CreateRouteDto
{
    public int Id { get; set; }
}

public class CreateRouteDto
{
    [Required]
    public string Type { get; set; } = null!;
}

public class UpdateRouteDto : CreateRouteDto
{
    [Required]
    public int Id { get; set; }
}

public class CreateRouteWithAddressesDto : CreateRouteDto
{
    [Required]
    [MinLength(2)]
    public IList<CreateRouteAddressWithAddressDto> RouteAddresses { get; set; } = null!;
}