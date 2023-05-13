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
    
    public int GetTotalEnrollmentCount(DateTime fromDate, DateTime toDate)
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteEnrollmentCount(fromDate, toDate, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalCanceledEnrollmentCount(DateTime fromDate, DateTime toDate)
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteCanceledEnrollmentCount(fromDate, toDate, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalSoldTicketCount(DateTime fromDate, DateTime toDate)
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteSoldTicketCount(fromDate, toDate, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalReturnedTicketCount(DateTime fromDate, DateTime toDate)
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteReturnedTicketCount(fromDate, toDate, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalIndirectTicketCount(DateTime fromDate, DateTime toDate)
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteIndirectTicketCount(fromDate, toDate, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalReturnedIndirectTicketCount(DateTime fromDate, DateTime toDate)
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteReturnedIndirectTicketCount(fromDate, toDate, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public double GetTotalRevenue(DateTime fromDate, DateTime toDate)
    {
        double result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetRouteTotalRevenue(fromDate, toDate, enrollment.RouteId);
            }
        }

        return result;
    }
    
    public double GetTotalAverageRating(DateTime fromDate, DateTime toDate)
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
                
                result += vehicle.GetRouteAverageRating(fromDate, toDate, enrollment.RouteId);
                enrollmentCount++;
            }
        }

        result /= enrollmentCount;
        result = !Double.IsNaN(result) ? Math.Round(result, 3) : 0;
        
        return result;
    }
}