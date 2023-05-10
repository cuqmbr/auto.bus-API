using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.Responses;

namespace Server.Services;

public class VehicleEnrollmentSearchService
{
    private readonly ApplicationDbContext _dbContext;

    public VehicleEnrollmentSearchService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IList<EnrollmentGroup> result)>
        GetRoute(int fromCityId, int toCityId, DateTime date)
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
        
        // Find routes without transfers
        
        var directEnrollments = new List<VehicleEnrollment>();

        foreach (var e in dbEnrollments)
        {
            if (e.Route.RouteAddresses.Count(ra => ra.Address.CityId == fromCityId) == 0 ||
                e.Route.RouteAddresses.Count(ra => ra.Address.CityId == toCityId) == 0)
            {
                    continue;
            }
            
            var fromOrder = e.Route.RouteAddresses.First(ra => ra.Address.CityId == fromCityId).Order;
            var toOrder = e.Route.RouteAddresses.First(ra => ra.Address.CityId == toCityId).Order;
                
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
            
            var fromOrder = routeAddresses.First(ra => ra.Address.CityId == fromCityId).Order;
            var toOrder = routeAddresses.First(ra => ra.Address.CityId == toCityId).Order;

            directEnrollments[directEnrollments.IndexOf(de)].Route.RouteAddresses = routeAddresses
                .OrderBy(ra => ra.Order)
                .SkipWhile(ra => ra.Order < fromOrder)
                .TakeWhile(ra => ra.Order <= toOrder)
                .ToList();

            directEnrollments[directEnrollments.IndexOf(de)]
                .DepartureDateTimeUtc = GetDepartureTime(de);
        }
        
        // Find routes with one transfer
        
        // Find enrollments with departure city
        
        var enrollmentsWithFrom = new List<VehicleEnrollment>();
        
        foreach (var e in dbEnrollments)
        {
            if (e.Route.RouteAddresses.Count(ra => ra.Address.CityId == fromCityId) == 0)
            {
                continue;
            }
            
            if (e.Route.RouteAddresses.Any(ra => ra.Address.CityId == fromCityId))
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

            var fromOrder = routeAddresses.First(ra => ra.Address.CityId == fromCityId).Order;

            enrollmentsWithFrom[enrollmentsWithFrom.IndexOf(ef)].Route.RouteAddresses = routeAddresses
                .OrderBy(ra => ra.Order)
                .SkipWhile(ra => ra.Order < fromOrder).ToList();
        }
        
        // Find enrollments with arrival city
        
        var enrollmentsWithTo = new List<VehicleEnrollment>();
        
        foreach (var e in dbEnrollments)
        {
            if (e.Route.RouteAddresses.Count(ra => ra.Address.CityId == toCityId) == 0)
            {
                continue;
            }
            
            if (e.Route.RouteAddresses.Any(ra => ra.Address.CityId == toCityId))
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

            var toOrder = routeAddresses.First(ra => ra.Address.CityId == toCityId).Order;

            enrollmentsWithTo[enrollmentsWithTo.IndexOf(et)].Route.RouteAddresses = routeAddresses
                .OrderBy(ra => ra.Order)
                .TakeWhile(ra => ra.Order <= toOrder).ToList();
        }
        
        // Find intersection of
        // enrollments with only departure city and
        // enrollments with only arrival city
        
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

                if (intersectionAddressId == null)
                {
                    continue;
                }

                var toOrder = efRouteAddresses.First(ra => ra.AddressId == intersectionAddressId).Order;
                var fromOrder = etRouteAddresses.First(ra => ra.AddressId == intersectionAddressId).Order;

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
        
        // Combine enrollments with transfers and enrollments without transfers

        foreach (var directEnrollment in directEnrollments)
        {
            oneTransferPath.Add(new List<VehicleEnrollment> {directEnrollment});
        }
        
        // Form an object that will be returned
        
        var result = new SearchEnrollmentResponse();

        foreach (var path in oneTransferPath)
        {
            var enrollmentGroup = new EnrollmentGroup();
            int j = 1;
            foreach (var vehicleEnrollment in path)
            {
                enrollmentGroup.Enrollments.Add(new FlattenedEnrollment
                {
                    Id = vehicleEnrollment.Id,
                    
                    DepartureAddressId = vehicleEnrollment.Route.RouteAddresses.First().AddressId,
                    DepartureTime = GetDepartureTime(vehicleEnrollment),
                    DepartureAddressName = vehicleEnrollment.Route.RouteAddresses.First().Address.Name,
                    DepartureCityName = vehicleEnrollment.Route.RouteAddresses.First().Address.City.Name,
                    DepartureStateName = vehicleEnrollment.Route.RouteAddresses.First().Address.City.State.Name,
                    DepartureCountryName = vehicleEnrollment.Route.RouteAddresses.First().Address.City.State.Country.Name,
                    DepartureAddressFullName = vehicleEnrollment.Route.RouteAddresses.First().Address.GetFullName(),
                    
                    ArrivalAddressId = vehicleEnrollment.Route.RouteAddresses.Last().AddressId,
                    ArrivalTime = GetArrivalTime(vehicleEnrollment),
                    ArrivalAddressName = vehicleEnrollment.Route.RouteAddresses.Last().Address.Name,
                    ArrivalCityName = vehicleEnrollment.Route.RouteAddresses.Last().Address.City.Name,
                    ArrivalStateName = vehicleEnrollment.Route.RouteAddresses.Last().Address.City.State.Name,
                    ArrivalCountryName = vehicleEnrollment.Route.RouteAddresses.Last().Address.City.State.Country.Name,
                    ArrivalAddressFullName = vehicleEnrollment.Route.RouteAddresses.Last().Address.GetFullName(),
                    
                    Order = j,
                    
                    VehicleId = vehicleEnrollment.VehicleId,
                    VehicleNumber = vehicleEnrollment.Vehicle.Number,
                    VehicleType = vehicleEnrollment.Vehicle.Type,
                    
                    CompanyId = vehicleEnrollment.Vehicle.CompanyId,
                    CompanyName = vehicleEnrollment.Vehicle.Company.Name
                });

                j++;
            }

            enrollmentGroup.Cost = GetTotalCost(path);
            enrollmentGroup.Duration = GetTotalDuration(path);
            
            result.EnrollmentGroups.Add(enrollmentGroup);
        }

        if (result.EnrollmentGroups.Count == 0)
        {
            return (false, new NotFoundResult(), null!);
        }
        
        return (true, null, result.EnrollmentGroups);

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