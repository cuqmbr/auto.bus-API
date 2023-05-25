using System.ComponentModel.DataAnnotations;

namespace SharedModels.DataTransferObjects.Model;

public class TicketGroupDto
{
    public int Id { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime PurchaseDateTime { get; set; }
    public bool IsReturned { get; set; } = false;
    public bool IsPurchased { get; set; }
    
    public string DepartureAddressName { get; set; } = null!;
    public string DepartureCityName { get; set; } = null!;
    public string DepartureStateName { get; set; } = null!;
    public string DepartureCountryName { get; set; } = null!;
    public string DepartureFullName { get; set; } = null!;
    public DateTime DepartureDateTime { get; set; }
    
    public string ArrivalAddressName { get; set; } = null!;
    public string ArrivalCityName { get; set; } = null!;
    public string ArrivalStateName { get; set; } = null!;
    public string ArrivalCountryName { get; set; } = null!;
    public string ArrivalFullName { get; set; } = null!;
    public DateTime ArrivalDateTime { get; set; }
    
    public double Cost { get; set; }
    
    public IList<TicketDto> Tickets { get; set; } = null!;
}