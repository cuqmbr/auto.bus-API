using System.ComponentModel.DataAnnotations;

namespace Server.Models;

public class Route
{
    [Key]
    public int Id { get; set; }
    
    public string Type { get; set; } = null!;

    public virtual IList<RouteAddress> RouteAddresses { get; set; } = null!;
    public virtual IList<VehicleEnrollment> VehicleEnrollments { get; set; } = null!;
    
    public int GetCompanyEnrollmentCount(DateTime fromDate, DateTime toDate, int companyId)
    {
        return VehicleEnrollments.Count(ve =>
            !ve.IsCanceled &&
            ve.DepartureDateTimeUtc >= fromDate && ve.DepartureDateTimeUtc <= toDate &&
            ve.Vehicle.CompanyId == companyId);
    }

    public int GetCompanyCanceledEnrollmentCount(DateTime fromDate, DateTime toDate, int companyId)
    {
        return VehicleEnrollments.Count(ve =>
            ve.IsCanceled &&
            ve.DepartureDateTimeUtc >= fromDate && ve.DepartureDateTimeUtc <= toDate &&
            ve.Vehicle.CompanyId == companyId);
    }
    
    public int GetCompanySoldTicketCount(DateTime fromDate, DateTime toDate, int companyId)
    {
        int result = 0;

        foreach (var enrollment in VehicleEnrollments)
        {
            result += enrollment.Tickets.Count(t =>
                !t.TicketGroup.IsReturned &&
                t.VehicleEnrollment.DepartureDateTimeUtc >= fromDate && t.VehicleEnrollment.DepartureDateTimeUtc <= toDate &&
                t.VehicleEnrollment.Vehicle.CompanyId == companyId);
        }

        return result;
    }

    public int GetCompanyIndirectTicketCount(DateTime fromDate, DateTime toDate, int companyId)
    {
        int result = 0;

        int departureAddressId = RouteAddresses.First().AddressId;
        int arrivalAddressId = RouteAddresses.Last().AddressId;
        
        foreach (var enrollment in VehicleEnrollments)
        {
            result += enrollment.Tickets.Count(t => !t.TicketGroup.IsReturned &&
                t.FirstRouteAddressId != departureAddressId ||
                t.LastRouteAddressId != arrivalAddressId &&
                t.VehicleEnrollment.DepartureDateTimeUtc >= fromDate && t.VehicleEnrollment.DepartureDateTimeUtc <= toDate &&
                t.VehicleEnrollment.Vehicle.CompanyId == companyId);
        }

        return result;
    }

    public double GetCompanyTotalRevenue(DateTime fromDate, DateTime toDate, int companyId)
    {
        double result = 0;

        var enrollments = VehicleEnrollments.Where(ve =>
            ve.DepartureDateTimeUtc >= fromDate && ve.DepartureDateTimeUtc <= toDate &&
            ve.Vehicle.CompanyId == companyId);
        
        foreach (var enrollment in enrollments)
        {
            foreach (var ticket in enrollment.Tickets)
            {
                result += ticket.GetCost();
            }
        }

        return result;
    }

    public double GetCompanyAverageRating(DateTime fromDate, DateTime toDate, int companyId)
    {
        double result = 0;
        int reviewCount = 0;
        
        var enrollments = VehicleEnrollments.Where(ve =>
            ve.DepartureDateTimeUtc >= fromDate && ve.DepartureDateTimeUtc <= toDate &&
            ve.Vehicle.CompanyId == companyId);

        foreach (var enrollment in enrollments)
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
