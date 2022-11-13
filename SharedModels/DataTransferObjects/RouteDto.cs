using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class RouteDto : CreateRouteDto
{
    public int Id { get; set; }
    
    public virtual IList<InRouteRouteAddressDto> RouteAddresses { get; set; } = null!;
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