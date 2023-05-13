using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class Company
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; } = null!;
    
    [ForeignKey("OwnerId")]
    public string OwnerId { get; set; }  = null!;
    public User Owner { get; set; } = null!;

    public virtual IList<Vehicle> Vehicles { get; set; } = null!;
    public virtual IList<CompanyDriver> CompanyDrivers { get; set; } = null!;
    
    public int GetTotalEnrollmentCount(DateTime fromDateUtc, DateTime toDateUtc)
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteEnrollmentCount(fromDateUtc, toDateUtc, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalCanceledEnrollmentCount(DateTime fromDateUtc, DateTime toDateUtc)
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteCanceledEnrollmentCount(fromDateUtc, toDateUtc, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalSoldTicketCount(DateTime fromDateUtc, DateTime toDateUtc)
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteSoldTicketCount(fromDateUtc, toDateUtc, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalReturnedTicketCount(DateTime fromDateUtc, DateTime toDateUtc)
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteReturnedTicketCount(fromDateUtc, toDateUtc, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalIndirectTicketCount(DateTime fromDateUtc, DateTime toDateUtc)
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteIndirectTicketCount(fromDateUtc, toDateUtc, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalReturnedIndirectTicketCount(DateTime fromDateUtc, DateTime toDateUtc)
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteReturnedIndirectTicketCount(fromDateUtc, toDateUtc, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public double GetTotalRevenue(DateTime fromDateUtc, DateTime toDateUtc)
    {
        double result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteTotalRevenue(fromDateUtc, toDateUtc, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public double GetTotalAverageRating(DateTime fromDateUtc, DateTime toDateUtc)
    {
        double result = 0;
        int enrollmentCount = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                if (enrollment.Reviews.Count == 0)
                {
                    continue;
                }
                
                result += vehicle.GetRouteAverageRating(fromDateUtc, toDateUtc, enrollment.RouteId);
                enrollmentCount++;
            }
        }

        result /= enrollmentCount;
        result = !Double.IsNaN(result) ? Math.Round(result, 3) : 0;
        
        return result;
    }
}