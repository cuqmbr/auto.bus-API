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
    
    public int GetTotalEnrollmentCount()
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetEnrollmentCount(enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalCanceledEnrollmentCount()
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetCanceledEnrollmentCount(enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalSoldTicketCount()
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetSoldTicketCount(enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalReturnedTicketCount()
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetReturnedTicketCount(enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalIndirectTicketCount()
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetIndirectTicketCount(enrollment.RouteId);
            }
        }

        return result;
    }
    
    public int GetTotalReturnedIndirectTicketCount()
    {
        int result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetReturnedIndirectTicketCount(enrollment.RouteId);
            }
        }

        return result;
    }
    
    public double GetTotalRevenue()
    {
        double result = 0;

        foreach (var vehicle in Vehicles)
        {
            foreach (var enrollment in vehicle.VehicleEnrollments)
            {
                result += vehicle.GetTotalRevenue(enrollment.RouteId);
            }
        }

        return result;
    }
    
    public double GetTotalAverageRating()
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
                
                result += vehicle.GetAverageRating(enrollment.RouteId);
                enrollmentCount++;
            }
        }

        result /= enrollmentCount;
        result = !Double.IsNaN(result) ? Math.Round(result, 3) : 0;
        
        return result;
    }
}