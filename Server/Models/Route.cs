using System.ComponentModel.DataAnnotations;

namespace Server.Models;

public class Route
{
    [Key]
    public int Id { get; set; }
    
    public string Type { get; set; } = null!;

    public virtual IList<RouteAddress> RouteAddresses { get; set; } = null!;
    public virtual IList<VehicleEnrollment> VehicleEnrollments { get; set; } = null!;
    
    public int GetEnrollmentCount()
    {
        return VehicleEnrollments.Count(ve => !ve.IsCanceled);
    }

    public int GetCanceledEnrollmentCount()
    {
        return VehicleEnrollments.Count(ve => ve.IsCanceled);
    }
    
    public int GetSoldTicketCount()
    {
        int result = 0;

        foreach (var enrollment in VehicleEnrollments)
        {
            result += enrollment.Tickets.Count(t => !t.IsReturned);
        }

        return result;
    }

    public int GetIndirectTicketCount()
    {
        int result = 0;

        int departureAddressId = RouteAddresses.First().AddressId;
        int arrivalAddressId = RouteAddresses.Last().AddressId;
        
        foreach (var enrollment in VehicleEnrollments)
        {
            result += enrollment.Tickets.Count(t => !t.IsReturned &&
                t.FirstRouteAddressId != departureAddressId ||
                t.LastRouteAddressId != arrivalAddressId);
        }

        return result;
    }

    public double GetTotalRevenue()
    {
        double result = 0;

        foreach (var enrollment in VehicleEnrollments)
        {
            foreach (var ticket in enrollment.Tickets)
            {
                result += ticket.GetCost();
            }
        }

        return result;
    }

    public double GetAverageRating()
    {
        double result = 0;
        int reviewCount = 0;

        foreach (var enrollment in VehicleEnrollments)
        {
            if (enrollment.Reviews.Count == 0)
            {
                continue;
            }
            
            foreach (var review in enrollment.Reviews)
            {
                result += review.Rating;
                reviewCount += enrollment.Reviews.Count;  
            }
        }

        result /= reviewCount;
        result = !Double.IsNaN(result) ? Math.Round(result, 3) : 0;
        
        return result;
    }
}
