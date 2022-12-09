using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;

namespace Server.Services;

public class AutomationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ISortHelper<ExpandoObject> _sortHelper;

    public AutomationService(ApplicationDbContext dbContext, 
        ISortHelper<ExpandoObject> sortHelper)
    {
        _dbContext = dbContext;
        _sortHelper = sortHelper;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, List<ExpandoObject> result)> GetRoute(
        int from, int to, DateTime date)
    {
        var dbEnrollments = await _dbContext.VehicleEnrollments
            .Include(ve => ve.Tickets)
            .Include(ve => ve.Vehicle)
            .ThenInclude(v => v.Company)
            .Include(ve => ve.RouteAddressDetails)
            .Include(ve => ve.Route).ThenInclude(r => r.RouteAddresses)
            .ThenInclude(ra => ra.Address).ThenInclude(a => a.City)
            .ThenInclude(c => c.State).ThenInclude(s => s.Country)
            .Where(ve => ve.DepartureDateTimeUtc.Date >= date.Date &&
                         ve.DepartureDateTimeUtc.Date <= date.AddDays(3).Date)
            .ToListAsync();

        var toBeRemovedEnrollmentsIds = new List<int>();
        
        var directEnrollments = new List<VehicleEnrollment>();

        foreach (var e in dbEnrollments)
        {
            if (e.Route.RouteAddresses.Count(ra => ra.AddressId == from) == 0 ||
                e.Route.RouteAddresses.Count(ra => ra.AddressId == to) == 0)
            {
                    continue;
            }
            
            var fromOrder = e.Route.RouteAddresses.FirstOrDefault(rad => 
                rad.AddressId == from)?.Order;
            var toOrder = e.Route.RouteAddresses.FirstOrDefault(rad => 
                rad.AddressId == to)?.Order;
                
            if (fromOrder < toOrder)
            {
                directEnrollments.Add(e);
                toBeRemovedEnrollmentsIds.Add(e.Id);
            }
        }

        dbEnrollments.RemoveAll(e => toBeRemovedEnrollmentsIds.Any(id => id == e.Id));
        toBeRemovedEnrollmentsIds.Clear();

        foreach (var de in directEnrollments)
        {
            var routeAddresses = de.Route.RouteAddresses;
            
            var fromOrder = 
                routeAddresses.First(ra => ra.AddressId == from).Order;
            var toOrder = 
                routeAddresses.First(ra => ra.AddressId == to).Order;

            directEnrollments[directEnrollments.IndexOf(de)].Route.RouteAddresses = routeAddresses
                .OrderBy(ra => ra.Order)
                .SkipWhile(ra => ra.Order < fromOrder)
                .TakeWhile(ra => ra.Order <= toOrder)
                .ToList();

            directEnrollments[directEnrollments.IndexOf(de)]
                .DepartureDateTimeUtc = GetDepartureTime(de);
        }
        
        
        
        var enrollmentsWithFrom = new List<VehicleEnrollment>();
        
        foreach (var e in dbEnrollments)
        {
            if (e.Route.RouteAddresses.Count(ra => ra.AddressId == from) == 0)
            {
                continue;
            }
            
            if (e.Route.RouteAddresses.Any(ra => ra.AddressId == from))
            {
                enrollmentsWithFrom.Add(e);
                toBeRemovedEnrollmentsIds.Add(e.Id);
            }
        }

        // dbEnrollments.RemoveAll(e => ToBeRemovedEnrollmentsIds.Any(id => id == e.Id));
        toBeRemovedEnrollmentsIds.Clear();
        
        foreach (var ef in enrollmentsWithFrom)
        {
            var routeAddresses = ef.Route.RouteAddresses;

            var fromOrder = 
                routeAddresses.First(ra => ra.AddressId == from).Order;

            enrollmentsWithFrom[enrollmentsWithFrom.IndexOf(ef)].Route.RouteAddresses = routeAddresses
                .OrderBy(ra => ra.Order)
                .SkipWhile(ra => ra.Order < fromOrder).ToList();
        }
        
        
        var enrollmentsWithTo = new List<VehicleEnrollment>();
        
        foreach (var e in dbEnrollments)
        {
            if (e.Route.RouteAddresses.Count(ra => ra.AddressId == to) == 0)
            {
                continue;
            }
            
            if (e.Route.RouteAddresses.Any(ra => ra.AddressId == to))
            {
                enrollmentsWithTo.Add(e);
                toBeRemovedEnrollmentsIds.Add(e.Id);
            }
        }

        // dbEnrollments.RemoveAll(e => ToBeRemovedEnrollmentsIds.Any(id => id == e.Id));
        toBeRemovedEnrollmentsIds.Clear();
        
        foreach (var et in enrollmentsWithTo)
        {
            var routeAddresses = et.Route.RouteAddresses;

            var toOrder = 
                routeAddresses.First(ra => ra.AddressId == to).Order;

            enrollmentsWithTo[enrollmentsWithTo.IndexOf(et)].Route.RouteAddresses = routeAddresses
                .OrderBy(ra => ra.Order)
                .TakeWhile(ra => ra.Order <= toOrder).ToList();
        }
        
        
        var oneTransferPath = new List<List<VehicleEnrollment>>();

        foreach (var ef in enrollmentsWithFrom)
        {
            foreach (var et in enrollmentsWithTo)
            {
                var efRouteAddresses = ef.Route.RouteAddresses;
                var etRouteAddresses = et.Route.RouteAddresses;

                var intersectionAddressId = efRouteAddresses.IntersectBy(
                    etRouteAddresses.Select(x => x.AddressId),
                    x => x.AddressId).FirstOrDefault()?.AddressId;

                var toOrder = efRouteAddresses.First(ra => 
                        ra.AddressId == intersectionAddressId).Order;
                var fromOrder = etRouteAddresses.First(ra => 
                    ra.AddressId == intersectionAddressId).Order;

                enrollmentsWithFrom[enrollmentsWithFrom.IndexOf(ef)].Route.RouteAddresses =
                    efRouteAddresses.OrderBy(ra => ra.Order)
                        .TakeWhile(ra => ra.Order <= toOrder).ToList();
                enrollmentsWithTo[enrollmentsWithTo.IndexOf(et)].Route.RouteAddresses =
                    etRouteAddresses.OrderBy(ra => ra.Order)
                        .SkipWhile(ra => ra.Order < fromOrder).ToList();

                var fromArrivalTime = GetArrivalTime(ef);
                var toDepartureTime = GetDepartureTime(et);
                
                var doesIntersect = intersectionAddressId != null;
                
                if (doesIntersect && (toDepartureTime - fromArrivalTime) >= TimeSpan.FromMinutes(5))
                {
                    oneTransferPath.Add(new List<VehicleEnrollment> {ef, et});
                }
            }
        }

        foreach (var directEnrollment in directEnrollments)
        {
            oneTransferPath.Add(new List<VehicleEnrollment> {directEnrollment});
        }
        
        var result = new List<ExpandoObject>();

        int i = 1;
        foreach (var path in oneTransferPath)
        {
            
            var shapedPath = new ExpandoObject();
            var enrollmentGroup = new ExpandoObject();

            int j = 1;
            foreach (var vehicleEnrollment in path)
            {
                var enrollment = new ExpandoObject();

                enrollment.TryAdd("id", vehicleEnrollment.Id);
                enrollment.TryAdd("departureTime", GetDepartureTime(vehicleEnrollment));
                enrollment.TryAdd("arrivalTime", GetArrivalTime(vehicleEnrollment));
                enrollment.TryAdd("departureAddressName", vehicleEnrollment.Route.RouteAddresses.First().Address.Name);
                enrollment.TryAdd("departureAddressFullName", vehicleEnrollment.Route.RouteAddresses.First().Address.GetFullName());
                enrollment.TryAdd("departureAddressId", vehicleEnrollment.Route.RouteAddresses.First().AddressId);
                enrollment.TryAdd("arrivalAddressName", vehicleEnrollment.Route.RouteAddresses.Last().Address.Name);
                enrollment.TryAdd("arrivalAddressFullName", vehicleEnrollment.Route.RouteAddresses.Last().Address.GetFullName());
                enrollment.TryAdd("arrivalAddressId", vehicleEnrollment.Route.RouteAddresses.Last().AddressId);
                enrollment.TryAdd("order", j);

                var vehicle = new ExpandoObject();

                vehicle.TryAdd("type", vehicleEnrollment.Vehicle.Type);
                vehicle.TryAdd("number", vehicleEnrollment.Vehicle.Number);
                
                enrollment.TryAdd("vehicle", vehicle);
                
                var company = new ExpandoObject();

                company.TryAdd("name", vehicleEnrollment.Vehicle.Company.Name);
                
                enrollment.TryAdd("company", company);
                
                
                
                enrollmentGroup.TryAdd($"enrollment{j}", enrollment);
                
                j++;
            }

            enrollmentGroup.TryAdd("totalDuration", GetTotalDuration(path));
            enrollmentGroup.TryAdd("totalCost", GetTotalCost(path));
            
            shapedPath.TryAdd($"enrollmentGroup{i}", enrollmentGroup);
            result.Add(shapedPath);

            i++;
        }

        // result = _sortHelper.ApplySort(result[].AsQueryable(), "+cost").ToList();
        
        return (true, null, result);

        DateTime GetDepartureTime(VehicleEnrollment enrollment)
        {
            var departureDateTimeUtc = enrollment.DepartureDateTimeUtc;

            var departureRouteAddressId = enrollment.Route.RouteAddresses.First().Id;

            foreach (var detail in enrollment.RouteAddressDetails)
            {
                if (detail.RouteAddressId == departureRouteAddressId)
                {
                    departureDateTimeUtc += detail.WaitTimeSpan;
                    break;
                }
                
                departureDateTimeUtc += detail.TimeSpanToNextCity + detail.WaitTimeSpan;
            }

            return departureDateTimeUtc;
        }

        DateTime GetArrivalTime(VehicleEnrollment enrollment)
        {
            var arrivalDateTimeUtc = enrollment.DepartureDateTimeUtc;

            var arrivalRouteAddressId = enrollment.Route.RouteAddresses.Last().Id;

            foreach (var detail in enrollment.RouteAddressDetails)
            {
                if (detail.RouteAddressId == arrivalRouteAddressId)
                {
                    break;
                }
                
                arrivalDateTimeUtc += detail.TimeSpanToNextCity + detail.WaitTimeSpan;
            }

            return arrivalDateTimeUtc;
        }

        TimeSpan GetTotalDuration(List<VehicleEnrollment> vehicleEnrollments)
        {
            return GetArrivalTime(vehicleEnrollments.Last()) -
                   GetDepartureTime(vehicleEnrollments.First());
        }

        double GetTotalCost(List<VehicleEnrollment> vehicleEnrollments)
        {
            double result = 0;

            foreach (var enrollment in vehicleEnrollments)
            {
                foreach (var routeAddresses in enrollment.Route.RouteAddresses)
                {
                    if (enrollment.Route.RouteAddresses.Last().Id == routeAddresses.Id)
                    {
                        break;
                    }

                    result += routeAddresses.RouteAddressDetails.First(rad =>
                        rad.VehicleEnrollmentId == enrollment.Id).CostToNextCity;
                }
            }

            return result;
        }
    }
}