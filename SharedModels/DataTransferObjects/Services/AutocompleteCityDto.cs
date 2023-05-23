namespace SharedModels.DataTransferObjects.Services;

public class AutocompleteCityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string StateName { get; set; } = null!;
    public string CountryName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int Order { get; set; }
}