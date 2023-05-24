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
    
    public int GetRouteEnrollmentCount(DateTime fromDate, DateTime toDate, int routeId)
    {
        return VehicleEnrollments.Count(ve => 
            !ve.IsCancelled() &&
            ve.DepartureDateTimeUtc >= fromDate && ve.DepartureDateTimeUtc <= toDate &&
            ve.RouteId == routeId);
    }
    
    public int GetRouteCanceledEnrollmentCount(DateTime fromDate, DateTime toDate, int routeId)
    {
        return VehicleEnrollments.Count(ve =>
            ve.IsCancelled() &&
            ve.DepartureDateTimeUtc >= fromDate && ve.DepartureDateTimeUtc <= toDate &&
            ve.RouteId == routeId);
    }
    
    public int GetRouteSoldTicketCount(DateTime fromDate, DateTime toDate, int routeId)
    {
        int result = 0;

        var enrollments = VehicleEnrollments.Where(ve =>
            ve.RouteId == routeId &&
            ve.DepartureDateTimeUtc >= fromDate && ve.DepartureDateTimeUtc <= toDate);

        foreach (var enrollment in enrollments)
        {
            result += enrollment.Tickets.Count(t => !t.TicketGroup.IsReturned);
        }

        return result;
    }
    
    public int GetRouteReturnedTicketCount(DateTime fromDate, DateTime toDate, int routeId)
    {
        int result = 0;
        
        var enrollments = VehicleEnrollments.Where(ve =>
            ve.RouteId == routeId &&
            ve.DepartureDateTimeUtc >= fromDate && ve.DepartureDateTimeUtc <= toDate);

        foreach (var enrollment in enrollments)
        {
            result += enrollment.Tickets.Count(t => t.TicketGroup.IsReturned);
        }

        return result;
    }
    
    public int GetRouteIndirectTicketCount(DateTime fromDate, DateTime toDate, int routeId)
    {
        int result = 0;
        
        var enrollments = VehicleEnrollments.Where(ve =>
            ve.RouteId == routeId &&
            ve.DepartureDateTimeUtc >= fromDate && ve.DepartureDateTimeUtc <= toDate);

        foreach (var enrollment in enrollments)
        {
            var departureRouteAddressId = enrollment.Route.RouteAddresses.First().AddressId;
            var arrivalRouteAddressId = enrollment.Route.RouteAddresses.Last().AddressId;
            
            result += enrollment.Tickets.Count(t => !t.TicketGroup.IsReturned &&
                t.FirstRouteAddressId != departureRouteAddressId ||
                t.LastRouteAddressId != arrivalRouteAddressId);
        }

        return result;
    }
    
    public int GetRouteReturnedIndirectTicketCount(DateTime fromDate, DateTime toDate, int routeId)
    {
        int result = 0;
        
        var enrollments = VehicleEnrollments.Where(ve =>
            ve.RouteId == routeId &&
            ve.DepartureDateTimeUtc >= fromDate && ve.DepartureDateTimeUtc <= toDate);

        foreach (var enrollment in enrollments)
        {
            var departureRouteAddressId = enrollment.Route.RouteAddresses.First().AddressId;
            var arrivalRouteAddressId = enrollment.Route.RouteAddresses.Last().AddressId;
            
            result += enrollment.Tickets.Count(t => t.TicketGroup.IsReturned &&
                (t.FirstRouteAddressId != departureRouteAddressId ||
                t.LastRouteAddressId != arrivalRouteAddressId));
        }

        return result;
    }
    
    public double GetRouteTotalRevenue(DateTime fromDate, DateTime toDate, int routeId)
    {
        double result = 0;
        
        var enrollments = VehicleEnrollments.Where(ve =>
            ve.RouteId == routeId &&
            ve.DepartureDateTimeUtc >= fromDate && ve.DepartureDateTimeUtc <= toDate);

        foreach (var enrollment in enrollments)
        {
            foreach (var ticket in enrollment.Tickets)
            {
                result += ticket.GetCost();
            }
        }

        return result;
    }
    
    public double GetRouteAverageRating(DateTime fromDate, DateTime toDate, int routeId)
    {
        double result = 0;
        int reviewCount = 0;
        
        var enrollments = VehicleEnrollments.Where(ve =>
            ve.RouteId == routeId &&
            ve.DepartureDateTimeUtc >= fromDate && ve.DepartureDateTimeUtc <= toDate);

        foreach (var enrollment in enrollments)
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