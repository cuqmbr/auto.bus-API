using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects;

public class RouteDto
{
    public int Id { get; set; }
    
    public string Type { get; set; } = null!;
}

public class CreateRouteDto
{
    [Required]
    public string Type { get; set; } = null!;
}

public class UpdateRouteDto
{
    [Required]
    public int Id { get; set; }
    
    public string Type { get; set; } = null!;
}