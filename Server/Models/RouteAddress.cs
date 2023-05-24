using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class RouteAddress
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("RouteId")]
    public int RouteId { get; set; }
    public Route Route { get; set; } = null!;
    
    [ForeignKey("AddressId")]
    public int AddressId { get; set; }
    public Address Address { get; set; } = null!;
    
    [ForeignKey("RouteAddressDetailsId")]
    public int RouteAddressDetailsId { get; set; }
    public virtual IList<RouteAddressDetails> RouteAddressDetails { get; set; } = null!;
    
    public int Order { get; set; }

    public DateTime GetDepartureDateTimeUtc(int vehicleEnrollmentId)
    {
        var enrollment = Route.VehicleEnrollments.First(ve => ve.Id == vehicleEnrollmentId);
        DateTime result = enrollment.DepartureDateTimeUtc;

        foreach (var routeAddressDetail in enrollment.RouteAddressDetails)
        {
            if (routeAddressDetail.RouteAddressId != Route.RouteAddresses.First().Id &&
                routeAddressDetail.RouteAddressId != Route.RouteAddresses.Last().Id)
            {
                result += routeAddressDetail.WaitTimeSpan;
            }

            if (routeAddressDetail.RouteAddressId == Id)
            {
                break;
            }
            
            if (routeAddressDetail.RouteAddressId != Route.RouteAddresses.Last().Id)
            {
                result += routeAddressDetail.TimeSpanToNextCity;
            }
        }

        return result;
    }
    
    public DateTime GetArrivalDateTimeUtc(int vehicleEnrollmentId)
    {
        var enrollment = Route.VehicleEnrollments.First(ve => ve.Id == vehicleEnrollmentId);
        DateTime result = enrollment.DepartureDateTimeUtc;

        foreach (var routeAddressDetail in enrollment.RouteAddressDetails)
        {
            if (routeAddressDetail.RouteAddressId == Id)
            {
                break;
            }
            
            if (routeAddressDetail.RouteAddressId != Route.RouteAddresses.Last().Id)
            {
                result += routeAddressDetail.TimeSpanToNextCity;
            }

            if (routeAddressDetail.RouteAddressId != Route.RouteAddresses.First().Id &&
                routeAddressDetail.RouteAddressId != Route.RouteAddresses.Last().Id &&
                routeAddressDetail.RouteAddressId != Id)
            {
                result += routeAddressDetail.WaitTimeSpan;
            }
        }

        return result;
    }
}