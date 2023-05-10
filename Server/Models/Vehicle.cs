using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class Vehicle
{
    [Key]
    public int Id { get; set; }
    
    public string Number { get; set; } = null!;
    public string Type { get; set; } = null!;
    public int Capacity { get; set; }
    
    [ForeignKey("CompanyId")]
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    
    public bool HasClimateControl { get; set; }
    public bool HasWiFi { get; set; }
    public bool HasWC { get; set; }
    public bool HasStewardess { get; set; }
    public bool HasTV { get; set; }
    public bool HasOutlet { get; set; }
    public bool HasBelts { get; set; }

    public IList<VehicleEnrollment> VehicleEnrollments { get; set; } = null!;
    
    public int GetEnrollmentCount(int routeId)
    {
        return VehicleEnrollments.Count(ve => !ve.IsCanceled && ve.RouteId == routeId);
    }
    
    public int GetCanceledEnrollmentCount(int routeId)
    {
        return VehicleEnrollments.Count(ve => ve.IsCanceled && ve.RouteId == routeId);
    }
    
    public int GetSoldTicketCount(int routeId)
    {
        int result = 0;

        foreach (var enrollment in VehicleEnrollments.Where(ve => ve.RouteId == routeId))
        {
            result += enrollment.Tickets.Count(t => !t.IsReturned);
        }

        return result;
    }
    
    public int GetReturnedTicketCount(int routeId)
    {
        int result = 0;

        foreach (var enrollment in VehicleEnrollments.Where(ve => ve.RouteId == routeId))
        {
            result += enrollment.Tickets.Count(t => t.IsReturned);
        }

        return result;
    }
    
    public int GetIndirectTicketCount(int routeId)
    {
        int result = 0;

        foreach (var enrollment in VehicleEnrollments.Where(ve => ve.RouteId == routeId))
        {
            var departureRouteAddressId = enrollment.Route.RouteAddresses.First().AddressId;
            var arrivalRouteAddressId = enrollment.Route.RouteAddresses.Last().AddressId;
            
            result += enrollment.Tickets.Count(t => !t.IsReturned &&
                t.FirstRouteAddressId != departureRouteAddressId ||
                t.LastRouteAddressId != arrivalRouteAddressId);
        }

        return result;
    }
    
    public int GetReturnedIndirectTicketCount(int routeId)
    {
        int result = 0;

        foreach (var enrollment in VehicleEnrollments.Where(ve => ve.RouteId == routeId))
        {
            var departureRouteAddressId = enrollment.Route.RouteAddresses.First().AddressId;
            var arrivalRouteAddressId = enrollment.Route.RouteAddresses.Last().AddressId;
            
            result += enrollment.Tickets.Count(t => t.IsReturned &&
                (t.FirstRouteAddressId != departureRouteAddressId ||
                t.LastRouteAddressId != arrivalRouteAddressId));
        }

        return result;
    }
    
    public double GetTotalRevenue(int routeId)
    {
        double result = 0;

        foreach (var enrollment in VehicleEnrollments.Where(ve => ve.RouteId == routeId))
        {
            foreach (var ticket in enrollment.Tickets)
            {
                result += ticket.GetCost();
            }
        }

        return result;
    }
    
    public double GetAverageRating(int routeId)
    {
        double result = 0;
        int reviewCount = 0;

        foreach (var enrollment in VehicleEnrollments.Where(ve => ve.RouteId == routeId))
        {
            reviewCount += enrollment.Reviews.Count;
            
            foreach (var review in enrollment.Reviews)
            {
                result += review.Rating;
            }
        }

        result /= reviewCount;
        result = !Double.IsNaN(result) ? Math.Round(result, 3) : 0;

        return Math.Round(result, 3);
    }
}