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

namespace Server.Services;

public class TicketManagementService : ITicketManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _ticketSortHelper;
    private readonly IDataShaper<TicketDto> _ticketDataShaper;
    private readonly IPager<ExpandoObject> _pager;

    public TicketManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<ExpandoObject> ticketSortHelper, 
        IDataShaper<TicketDto> ticketDataShaper, IPager<ExpandoObject> pager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _ticketSortHelper = ticketSortHelper;
        _ticketDataShaper = ticketDataShaper;
        _pager = pager;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, TicketDto ticket)> AddTicket(CreateTicketDto createTicketDto)
    {
        var ticket = _mapper.Map<Ticket>(createTicketDto);
    
        await _dbContext.Tickets.AddAsync(ticket);
        await _dbContext.SaveChangesAsync();
    
        return (true, null, _mapper.Map<TicketDto>(ticket));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> tickets,
            PagingMetadata<ExpandoObject> pagingMetadata)> GetTickets(TicketParameters parameters)
    {
        var dbTickets = _dbContext.Tickets
            .Include(t => t.VehicleEnrollment)
            .AsQueryable();

        FilterByTicketPurchaseDateTime(ref dbTickets, parameters.FromPurchaseDateTimeUtc, 
            parameters.ToPurchaseDateTimeUtc);
        FilterByTicketReturnedState(ref dbTickets, parameters.IsReturned);
        FilterByTicketUserId(ref dbTickets, parameters.UserId);
        
        var ticketDtos = _mapper.ProjectTo<TicketDto>(dbTickets);
        var shapedData = _ticketDataShaper.ShapeData(ticketDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _ticketSortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null!, null!);
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);
        
        return (true, null, shapedData, pagingMetadata);

        void FilterByTicketPurchaseDateTime(ref IQueryable<Ticket> tickets,
            DateTime? fromDateTime, DateTime? toDateTime)
        {
            if (!tickets.Any() || fromDateTime == null || toDateTime == null)
            {
                return;
            }

            tickets = tickets.Where(t =>
                t.TicketGroup.PurchaseDateTimeUtc >= fromDateTime.Value.ToUniversalTime() &&
                t.TicketGroup.PurchaseDateTimeUtc <= toDateTime.Value.ToUniversalTime());
        }
        
        void FilterByTicketReturnedState(ref IQueryable<Ticket> tickets,
            bool? isReturned)
        {
            if (!tickets.Any() || !isReturned.HasValue)
            {
                return;
            }

            tickets = tickets.Where(t => t.TicketGroup.IsReturned == isReturned);
        }

        // TODO: change TicketParameters 
        void FilterByTicketUserId(ref IQueryable<Ticket> tickets,
            string? userId)
        {
            // if (!tickets.Any() || String.IsNullOrWhiteSpace(userId))
            // {
            //     return;
            // }
            //
            // tickets = tickets.Where(t =>
            //     t.UserId.ToLower().Contains(userId.ToLower()));
        }
    }
    
    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject ticket)> GetTicket(int id, string? fields)
    {
        if (!await IsTicketExists(id))
        {
            return (false, new NotFoundResult(), null!);
        }
        
        var dbTicket = await _dbContext.Tickets.Where(t => t.Id == id)
            .Include(t => t.VehicleEnrollment)
            .FirstAsync();

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = TicketParameters.DefaultFields;
        }
        
        var ticketDto = _mapper.Map<TicketDto>(dbTicket);
        var shapedData = _ticketDataShaper.ShapeData(ticketDto, fields);

        return (true, null, shapedData);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, TicketDto ticket)> UpdateTicket(UpdateTicketDto updateTicketDto)
    {
        var ticket = _mapper.Map<Ticket>(updateTicketDto);
        _dbContext.Entry(ticket).State = EntityState.Modified;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsTicketExists(updateTicketDto.Id))
            {
                return (false, new NotFoundResult(), null!);
            }
        }

        var dbTicket = await _dbContext.Tickets.FirstAsync(t => t.Id == ticket.Id);
        
        return (true, null, _mapper.Map<TicketDto>(dbTicket));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteTicket(int id)
    {
        var dbTicket = await _dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == id);
    
        if (dbTicket == null)
        {
            return (false, new NotFoundResult());
        }
    
        _dbContext.Tickets.Remove(dbTicket);
        await _dbContext.SaveChangesAsync();
    
        return (true, null);
    }

    public async Task<bool> IsTicketExists(int id)
    {
        return await _dbContext.Tickets.AnyAsync(t => t.Id == id);
    } 
}