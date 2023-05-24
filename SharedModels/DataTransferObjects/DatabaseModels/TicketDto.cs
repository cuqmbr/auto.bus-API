using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects.Model;

public class TicketDto
{
    public IList<InTicketAddress> Addresses { get; set; } = null!;
}

public class InTicketAddress
{
    public string Name { get; set; } = null!;
    public string CityName { get; set; } = null!;
    public string StateName { get; set; } = null!;
    public string CountryName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime ArrivalDateTime { get; set; }
    public DateTime DepartureDateTime { get; set; }
}