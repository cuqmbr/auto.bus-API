using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

public class Ticket
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("TicketGroupId")]
    public int TicketGroupId { get; set; }
    public TicketGroup TicketGroup { get; set; } = null!;
    
    [ForeignKey("VehicleEnrollmentId")]
    public int VehicleEnrollmentId { get; set; }
    public VehicleEnrollment VehicleEnrollment { get; set; } = null!;
    
    public DateTime PurchaseDateTimeUtc { get; set; } =  DateTime.UtcNow;
    public int FirstRouteAddressId { get; set; }
    public int LastRouteAddressId { get; set; }
    public bool IsReturned { get; set; } = false;
    public bool IsMissed { get; set; } = false;
    
    public double GetCost()
    {
        double cost = 0;

        var routeAddresses = VehicleEnrollment.Route.RouteAddresses
            .OrderBy(ra => ra.Order)
            .SkipWhile(ra => ra.AddressId != FirstRouteAddressId)
            .TakeWhile(ra => ra.AddressId != LastRouteAddressId)
            .ToArray();
    
        foreach (var routeAddress in routeAddresses)
        {
            var details = routeAddress.RouteAddressDetails
                .First(rad => rad.RouteAddressId == routeAddress.Id);
        
            cost += details.CostToNextCity;
        }
    
        return cost;
    }
    
    public DateTime GetDepartureTime()
    {
        var departureDateTimeUtc = VehicleEnrollment.DepartureDateTimeUtc;

        var routeAddresses = VehicleEnrollment.Route.RouteAddresses
            .OrderBy(ra => ra.Order);
        
        foreach (var routeAddress in routeAddresses)
        {
            var details = routeAddress.RouteAddressDetails
                .First(rad => rad.RouteAddressId == routeAddress.Id);
            
            if (routeAddress.AddressId == FirstRouteAddressId)
            {
                departureDateTimeUtc += details.WaitTimeSpan;
                break;
            }

            departureDateTimeUtc += details.TimeSpanToNextCity;
            departureDateTimeUtc += details.WaitTimeSpan;
        }

        return departureDateTimeUtc;
    }
    
    public DateTime GetArrivalTime()
    {
        var arrivalDateTimeUtc = VehicleEnrollment.DepartureDateTimeUtc;
        
        var routeAddresses = VehicleEnrollment.Route.RouteAddresses
            .OrderBy(ra => ra.Order);
        
        foreach (var routeAddress in routeAddresses)
        {
            var details = routeAddress.RouteAddressDetails
                .First(rad => rad.RouteAddressId == routeAddress.Id);
            
            if (routeAddress.AddressId == LastRouteAddressId)
            {
                break;
            }

            arrivalDateTimeUtc += details.TimeSpanToNextCity;
            arrivalDateTimeUtc += details.WaitTimeSpan;
        }

        return arrivalDateTimeUtc;
    }

    public Address GetDepartureAddress()
    {
        return VehicleEnrollment.Route.RouteAddresses
            .First(ra => ra.AddressId == FirstRouteAddressId)
            .Address;
    }

    public Address GetArrivalAddress()
    {
        return VehicleEnrollment.Route.RouteAddresses
            .First(ra => ra.AddressId == LastRouteAddressId)
            .Address;
    }
}