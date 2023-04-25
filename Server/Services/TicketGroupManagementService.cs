using System.Dynamic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public class TicketGroupManagementService : ITicketGroupManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _ticketGroupSortHelper;
    private readonly IDataShaper<TicketGroupDto> _ticketGroupDataShaper;
    private readonly IDataShaper<TicketGroupWithTicketsDto> _ticketGroupWithTicketsDataShaper;
    private readonly IPager<ExpandoObject> _pager;

    public TicketGroupManagementService(ApplicationDbContext dbContext, IMapper mapper, 
        ISortHelper<ExpandoObject> ticketGroupSortHelper, IDataShaper<TicketGroupDto> ticketGroupDataShaper, 
        IDataShaper<TicketGroupWithTicketsDto> ticketGroupWithTicketsDataShaper, IPager<ExpandoObject> pager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _ticketGroupSortHelper = ticketGroupSortHelper;
        _ticketGroupDataShaper = ticketGroupDataShaper;
        _ticketGroupWithTicketsDataShaper = ticketGroupWithTicketsDataShaper;
        _pager = pager;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, TicketGroupDto ticketGroup)> AddTicketGroup(CreateTicketGroupDto createTicketGroupDto)
    {
        var ticketGroup = _mapper.Map<TicketGroup>(createTicketGroupDto);
    
        await _dbContext.TicketGroups.AddAsync(ticketGroup);
        await _dbContext.SaveChangesAsync();
    
        return (true, null, _mapper.Map<TicketGroupDto>(ticketGroup));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, TicketGroupWithTicketsDto ticketGroup)> AddTicketGroupWithTickets(
        CreateTicketGroupWithTicketsDto createTicketGroupWithTicketsDto)
    {
        var ticketGroup = _mapper.Map<TicketGroup>(createTicketGroupWithTicketsDto);

        await _dbContext.TicketGroups.AddAsync(ticketGroup);
        await _dbContext.SaveChangesAsync();

        ticketGroup = await _dbContext.TicketGroups
            .Include(tg => tg.Tickets)
            .FirstAsync(tg => tg.Id == ticketGroup.Id);
        
        return (true, null, _mapper.Map<TicketGroupWithTicketsDto>(ticketGroup));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> ticketGroups,
        PagingMetadata<ExpandoObject> pagingMetadata)> 
        GetTicketGroups(TicketGroupParameters parameters)
    {
        var dbTicketGroups = _dbContext.TicketGroups.AsQueryable();

        FilterTicketGroupsByUserId(ref dbTicketGroups, parameters.UserId);
        
        var ticketGroupDtos = _mapper.ProjectTo<TicketGroupDto>(dbTicketGroups);
        var shapedData = _ticketGroupDataShaper.ShapeData(ticketGroupDtos, parameters.Fields).AsQueryable();

        try
        {
            shapedData = _ticketGroupSortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null!, null!);
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);
        
        return (true, null, shapedData, pagingMetadata);
        
        void FilterTicketGroupsByUserId(ref IQueryable<TicketGroup> ticketGroups,
            string? userId)
        {
            if (!ticketGroups.Any() || String.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            ticketGroups = ticketGroups.Where(tg => tg.UserId == userId);
        }
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> ticketGroups, 
        PagingMetadata<ExpandoObject> pagingMetadata)> 
        GetTicketGroupsWithTickets(TicketGroupWithTicketsParameters parameters)
    {
        var dbTicketGroups = _dbContext.TicketGroups
            .Include(tg => tg.Tickets)
            .ThenInclude(t => t.VehicleEnrollment)
            .AsQueryable();
        
        FilterTicketGroupsByUserId(ref dbTicketGroups, parameters.UserId);

        var ticketGroupDtos = _mapper.ProjectTo<TicketGroupWithTicketsDto>(dbTicketGroups); 
        var shapedData = _ticketGroupWithTicketsDataShaper.ShapeData(ticketGroupDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _ticketGroupSortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null, null)!;
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);

        return (true, null, shapedData, pagingMetadata);

        void FilterTicketGroupsByUserId(ref IQueryable<TicketGroup> ticketGroups,
            string? userId)
        {
            if (!ticketGroups.Any() || String.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            ticketGroups = ticketGroups.Where(tg => tg.UserId == userId);
        }
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject ticketGroup)> GetTicketGroup(int id, string? fields)
    {
        if (!await IsTicketGroupExist(id))
        {
            return (false, new NotFoundResult(), null)!;
        }
        
        var dbTicketGroup = await _dbContext.TicketGroups.Where(tg => tg.Id == id)
            .FirstAsync();

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = RouteParameters.DefaultFields;
        }
        
        var ticketGroupDto = _mapper.Map<TicketGroupDto>(dbTicketGroup);
        var shapedRouteData = _ticketGroupDataShaper.ShapeData(ticketGroupDto, fields);

        return (true, null, shapedRouteData);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject ticketGroup)> GetTicketGroupWithTickets(int id, string? fields)
    {
        if (!await IsTicketGroupExist(id))
        {
            return (false, new NotFoundResult(), null)!;
        }
        
        var dbTicketGroup = await _dbContext.TicketGroups.Where(tg => tg.Id == id)
            .Include(tg => tg.Tickets)
            .FirstAsync();

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = RouteParameters.DefaultFields;
        }
        
        var ticketGroupDto = _mapper.Map<TicketGroupDto>(dbTicketGroup);
        var shapedRouteData = _ticketGroupDataShaper.ShapeData(ticketGroupDto, fields);

        return (true, null, shapedRouteData);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, UpdateTicketGroupDto ticketGroup)> UpdateTicketGroup(UpdateTicketGroupDto updateTicketGroupDto)
    {
        var ticketGroup = _mapper.Map<TicketGroup>(updateTicketGroupDto);
        _dbContext.Entry(ticketGroup).State = EntityState.Modified;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsTicketGroupExist(updateTicketGroupDto.Id))
            {
                return (false, new NotFoundResult(), null)!;
            }
        }

        var dbTicketGroup = await _dbContext.TicketGroups.FirstAsync(r => r.Id == ticketGroup.Id);
        
        return (true, null, _mapper.Map<UpdateTicketGroupDto>(dbTicketGroup));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteTicketGroup(int id)
    {
        var dbTicketGroup = await _dbContext.TicketGroups.FirstOrDefaultAsync(tg => tg.Id == id);

        if (dbTicketGroup == null)
        {
            return (false, new NotFoundResult());
        }
       
        _dbContext.TicketGroups.Remove(dbTicketGroup);
        await _dbContext.SaveChangesAsync();
    
        return (true, null);
    }

    public async Task<bool> IsTicketGroupExist(int id)
    {
        return await _dbContext.TicketGroups.AnyAsync(tg => tg.Id == id);
    }
}