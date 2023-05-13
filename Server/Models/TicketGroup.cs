using System.ComponentModel.DataAnnotations;

namespace Server.Models;

public class TicketGroup
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;
    
    public virtual IList<Ticket> Tickets { get; set; }
    
    public double GetCost()
    {
        double cost = 0;

        foreach (var ticket in Tickets)
        {
            cost += ticket.GetCost();
        }

        return cost;
    }
    
    public DateTime GetDepartureTime()
    {
        var departureDateTimeUtc = Tickets.First().VehicleEnrollment.DepartureDateTimeUtc;

        var routeAddresses = Tickets.First().VehicleEnrollment.Route.RouteAddresses
            .OrderBy(ra => ra.Order).ToArray();
        
        foreach (var routeAddress in routeAddresses)
        {
            var details = routeAddress.RouteAddressDetails
                .First(rad => rad.RouteAddressId == routeAddress.Id);
            
            if (routeAddress.AddressId == Tickets.First().FirstRouteAddressId)
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
        var arrivalDateTimeUtc = Tickets.First().VehicleEnrollment.DepartureDateTimeUtc;
        
        var routeAddresses = Tickets.Last().VehicleEnrollment.Route.RouteAddresses
            .OrderBy(ra => ra.Order).ToArray();
        
        foreach (var routeAddress in routeAddresses)
        {
            var details = routeAddress.RouteAddressDetails
                .First(rad => rad.RouteAddressId == routeAddress.Id);
            
            if (routeAddress.AddressId == Tickets.Last().LastRouteAddressId)
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
        return Tickets.First().VehicleEnrollment.Route.RouteAddresses
            .First(ra => ra.AddressId == Tickets.First().FirstRouteAddressId)
            .Address;
    }

    public Address GetArrivalAddress()
    {
        return Tickets.Last().VehicleEnrollment.Route.RouteAddresses
            .First(ra => ra.AddressId == Tickets.Last().LastRouteAddressId)
            .Address;
    }
}