using System.Dynamic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;
using Utils;

namespace Server.Services;

public class TicketGroupManagementService : ITicketGroupManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _ticketGroupSortHelper;
    private readonly IDataShaper<TicketGroupDto> _ticketGroupDataShaper;
    private readonly IPager<ExpandoObject> _pager;
    private readonly ISessionUserService _sessionUserService;

    public TicketGroupManagementService(ApplicationDbContext dbContext, IMapper mapper, 
        ISortHelper<ExpandoObject> ticketGroupSortHelper, IDataShaper<TicketGroupDto> ticketGroupDataShaper, 
        IPager<ExpandoObject> pager, ISessionUserService sessionUserService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _ticketGroupSortHelper = ticketGroupSortHelper;
        _ticketGroupDataShaper = ticketGroupDataShaper;
        _pager = pager;
        _sessionUserService = sessionUserService;
    }

    public async Task<(bool isSucceed, IActionResult actionResult, IEnumerable<ExpandoObject> ticketGroups, PagingMetadata<ExpandoObject> pagingMetadata)>
        GetTicketGroups(TicketGroupParameters parameters)
    {
        var dbTicketGroups = _dbContext.TicketGroups.Include(tg => tg.Tickets)
            .ThenInclude(t => t.VehicleEnrollment).ThenInclude(ve => ve.Route)
            .ThenInclude(ra => ra.RouteAddresses).ThenInclude(ra => ra.Address)
            .ThenInclude(a => a.City).ThenInclude(c => c.State)
            .ThenInclude(s => s.Country).Include(tg => tg.Tickets)
            .ThenInclude(t => t.VehicleEnrollment).ThenInclude(ve => ve.RouteAddressDetails)
            .AsQueryable();
        
        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString())
        {
            dbTicketGroups = dbTicketGroups.Where(tg => tg.UserId == _sessionUserService.GetAuthUserId());
        }

        if (!dbTicketGroups.Any())
        {
            return (false, new NotFoundResult(), null!, null!);
        }

        FilterTicketGroupsByUserId(ref dbTicketGroups, parameters.UserId);
        FilterTicketGroupsByFromPurchaseDateTime(ref dbTicketGroups, parameters.FromPurchaseDateTime);
        FilterTicketGroupsByToPurchaseDateTime(ref dbTicketGroups, parameters.ToPurchaseDateTime);
        FilterTicketGroupsByReturnStatus(ref dbTicketGroups, parameters.IsReturned);

        var ticketGroupDtos = new List<TicketGroupDto>();
        foreach (var ticketGroup in await dbTicketGroups.ToArrayAsync())
        {
            ticketGroupDtos.Add(CreateTicketGroupDto(ticketGroup));
        }
        
        var shapedData = _ticketGroupDataShaper.ShapeData(ticketGroupDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _ticketGroupSortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null, null)!;
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber, parameters.PageSize);

        return (true, null!, shapedData, pagingMetadata);

        void FilterTicketGroupsByUserId(ref IQueryable<TicketGroup> ticketGroups, string? userId)
        {
            if (!ticketGroups.Any() || String.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            ticketGroups = ticketGroups.Where(tg => tg.UserId == userId);
        }

        void FilterTicketGroupsByFromPurchaseDateTime(ref IQueryable<TicketGroup> ticketGroups, DateTime? fromDateTime)
        {
            if (!ticketGroups.Any() || fromDateTime == null)
            {
                return;
            }

            ticketGroups = ticketGroups.Where(rg => rg.PurchaseDateTimeUtc >= fromDateTime);
        }
        
        void FilterTicketGroupsByToPurchaseDateTime(ref IQueryable<TicketGroup> ticketGroups, DateTime? toDateTime)
        {
            if (!ticketGroups.Any() || toDateTime == null)
            {
                return;
            }

            ticketGroups = ticketGroups.Where(rg => rg.PurchaseDateTimeUtc <= toDateTime);
        }
        
        void FilterTicketGroupsByReturnStatus(ref IQueryable<TicketGroup> ticketGroups, bool? isReturned)
        {
            if (!ticketGroups.Any() || isReturned == null)
            {
                return;
            }

            ticketGroups = ticketGroups.Where(rg => rg.IsReturned == isReturned);
        }
    }

    public async Task<(bool isSucceed, IActionResult actionResult, ExpandoObject ticketGroup)>
        GetTicketGroup(int id, string? fields)
    {
        if (!await IsTicketGroupExist(id))
        {
            return (false, new NotFoundResult(), null)!;
        }
        
        var dbTicketGroup = await _dbContext.TicketGroups.Where(tg => tg.Id == id)
            .Include(tg => tg.Tickets).ThenInclude(t => t.VehicleEnrollment)
            .ThenInclude(ve =>  ve.Route).ThenInclude(ra => ra.RouteAddresses)
            .ThenInclude(ra => ra.Address).ThenInclude(a => a.City)
            .ThenInclude(c => c.State).ThenInclude(s => s.Country)
            .Include(tg => tg.Tickets).ThenInclude(t => t.VehicleEnrollment)
            .ThenInclude(ve => ve.RouteAddressDetails).FirstAsync();
        
        if (_sessionUserService.GetAuthUserRole() != Identity.Roles.Administrator.ToString() &&
            dbTicketGroup.UserId != _sessionUserService.GetAuthUserId())
        {
            return (false, new UnauthorizedResult(), null!);
        }

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = TicketGroupParameters.DefaultFields;
        }

        var shapedTicketGroupData = _ticketGroupDataShaper.ShapeData(CreateTicketGroupDto(dbTicketGroup), fields);

        return (true, null!, shapedTicketGroupData);
    }

    TicketGroupDto CreateTicketGroupDto(TicketGroup ticketGroup)
    {
        var inGroupTicketDtos = new List<TicketDto>();
        foreach (var ticket in ticketGroup.Tickets)
        {
            var inTicketAddressDtos = new List<InTicketAddress>();

            var inTicketRouteAddresses = ticket.VehicleEnrollment.Route.RouteAddresses.OrderBy(ra => ra.Order)
                .SkipWhile(ra => ra.AddressId != ticket.FirstRouteAddressId)
                .TakeWhile(ra => ra.AddressId != ticket.LastRouteAddressId)
                .ToList();
            inTicketRouteAddresses.Add(ticket.VehicleEnrollment.Route.RouteAddresses.First(ra => ra.AddressId == ticket.LastRouteAddressId));
            
            foreach (var routeAddress in inTicketRouteAddresses)
            {
                inTicketAddressDtos.Add(new InTicketAddress
                {
                    Name = routeAddress.Address.Name,
                    CityName = routeAddress.Address.City.Name,
                    StateName = routeAddress.Address.City.State.Name,
                    CountryName = routeAddress.Address.City.State.Country.Name,
                    FullName = routeAddress.Address.GetFullName(),
                    Latitude = routeAddress.Address.Latitude,
                    Longitude = routeAddress.Address.Longitude,
                    DepartureDateTime = routeAddress.GetDepartureDateTimeUtc(ticket.VehicleEnrollmentId),
                    ArrivalDateTime = routeAddress.GetArrivalDateTimeUtc(ticket.VehicleEnrollmentId)
                });
            }
            
            inGroupTicketDtos.Add(new TicketDto
            {
                Addresses = inTicketAddressDtos
            });
        }

        var ticketGroupDto = new TicketGroupDto
        {
            Id = ticketGroup.Id,
            IsReturned = ticketGroup.IsReturned,
            PurchaseDateTime = ticketGroup.PurchaseDateTimeUtc,
            DepartureAddressName = ticketGroup.Tickets.First().GetDepartureAddress().Name,
            DepartureCityName = ticketGroup.Tickets.First().GetDepartureAddress().City.Name,
            DepartureStateName = ticketGroup.Tickets.First().GetDepartureAddress().City.State.Name,
            DepartureCountryName = ticketGroup.Tickets.First().GetDepartureAddress().City.State.Country.Name,
            DepartureFullName = ticketGroup.Tickets.First().GetDepartureAddress().GetFullName(),
            DepartureDateTime = ticketGroup.Tickets.First().GetDepartureTime(),
            ArrivalAddressName = ticketGroup.Tickets.First().GetArrivalAddress().Name,
            ArrivalCityName = ticketGroup.Tickets.First().GetArrivalAddress().City.Name,
            ArrivalStateName = ticketGroup.Tickets.First().GetArrivalAddress().City.State.Name,
            ArrivalCountryName = ticketGroup.Tickets.First().GetArrivalAddress().City.State.Country.Name,
            ArrivalFullName = ticketGroup.Tickets.First().GetArrivalAddress().GetFullName(),
            ArrivalDateTime = ticketGroup.Tickets.Last().GetArrivalTime(),
            Cost = ticketGroup.GetCost(),
            Tickets = inGroupTicketDtos
        };

        return ticketGroupDto;
    }

    private async Task<bool> IsTicketGroupExist(int id)
    {
        return await _dbContext.TicketGroups.AnyAsync(tg => tg.Id == id);
    }
}