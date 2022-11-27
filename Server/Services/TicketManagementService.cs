using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public class TicketManagementService : ITicketManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<Ticket> _ticketSortHelper;
    private readonly IDataShaper<Ticket> _ticketDataShaper;

    public TicketManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<Ticket> ticketSortHelper, 
        IDataShaper<Ticket> ticketDataShaper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _ticketSortHelper = ticketSortHelper;
        _ticketDataShaper = ticketDataShaper;
    }

    public async Task<(bool isSucceed, string message, TicketDto ticket)> AddTicket(CreateTicketDto createTicketDto)
    {
        var ticket = _mapper.Map<Ticket>(createTicketDto);
    
        await _dbContext.Tickets.AddAsync(ticket);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty, _mapper.Map<TicketDto>(ticket));
    }

    public async Task<(bool isSucceed, string message, IEnumerable<TicketDto> tickets,
            PagingMetadata<Ticket> pagingMetadata)> GetTickets(TicketParameters parameters)
    {
        var dbTickets = _dbContext.Tickets
            .AsQueryable();

        FilterByTicketPurchaseDateTime(ref dbTickets, parameters.FromPurchaseDateTimeUtc, 
            parameters.ToPurchaseDateTimeUtc);
        FilterByTicketReturnedState(ref dbTickets, parameters.IsReturned);
        FilterByTicketUserId(ref dbTickets, parameters.UserId);

        try
        {
            dbTickets = _ticketSortHelper.ApplySort(dbTickets, parameters.Sort);
            
            // By calling Any() we will check if LINQ to Entities Query will be
            // executed. If not it will throw an InvalidOperationException exception
            var isExecuted = dbTickets.Any();
        }
        catch (Exception e)
        {
            return (false, "Invalid sorting string", null, null)!;
        }

        var pagingMetadata = ApplyPaging(ref dbTickets, parameters.PageNumber,
            parameters.PageSize);

        var shapedTicketsData = _ticketDataShaper.ShapeData(dbTickets, parameters.Fields);
        var ticketDtos = shapedTicketsData.ToList().ConvertAll(t => _mapper.Map<TicketDto>(t));
        
        return (true, "", ticketDtos, pagingMetadata);

        void FilterByTicketPurchaseDateTime(ref IQueryable<Ticket> tickets,
            DateTime? fromDateTime, DateTime? toDateTime)
        {
            if (!tickets.Any() || fromDateTime == null || toDateTime == null)
            {
                return;
            }

            tickets = tickets.Where(t =>
                t.PurchaseDateTimeUtc >= fromDateTime.Value.ToUniversalTime() &&
                t.PurchaseDateTimeUtc <= toDateTime.Value.ToUniversalTime());
        }
        
        void FilterByTicketReturnedState(ref IQueryable<Ticket> tickets,
            bool? isReturned)
        {
            if (!tickets.Any() || !isReturned.HasValue)
            {
                return;
            }

            tickets = tickets.Where(t => t.IsReturned == isReturned);
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

        PagingMetadata<Ticket> ApplyPaging(ref IQueryable<Ticket> tickets,
            int pageNumber, int pageSize)
        {
            var metadata = new PagingMetadata<Ticket>(tickets,
                pageNumber, pageSize);
            
            tickets = tickets
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return metadata;
        }
    }
    
    public async Task<(bool isSucceed, string message, TicketDto ticket)> GetTicket(int id, string? fields)
    {
        var dbTicket = await _dbContext.Tickets.Where(t => t.Id == id)
            .FirstOrDefaultAsync();

        if (dbTicket == null)
        {
            return (false, $"Ticket doesn't exist", null)!;
        }
        
        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = TicketParameters.DefaultFields;
        }
        
        var shapedTicketData = _ticketDataShaper.ShapeData(dbTicket, fields);
        var ticketDto = _mapper.Map<TicketDto>(shapedTicketData);

        return (true, "", ticketDto);
    }

    public async Task<(bool isSucceed, string message, UpdateTicketDto ticket)> UpdateTicket(UpdateTicketDto updateTicketDto)
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
                return (false, $"Ticket with id:{updateTicketDto.Id} doesn't exist", null)!;
            }
            
            throw;
        }

        var dbTicket = await _dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == ticket.Id);
        
        return (true, String.Empty, _mapper.Map<UpdateTicketDto>(dbTicket));
    }

    public async Task<(bool isSucceed, string message)> DeleteTicket(int id)
    {
        var dbTicket = await _dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == id);
    
        if (dbTicket == null)
        {
            return (false, $"Ticket with id:{id} doesn't exist");
        }
    
        _dbContext.Tickets.Remove(dbTicket);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty);
    }

    public async Task<bool> IsTicketExists(int id)
    {
        return await _dbContext.Tickets.AnyAsync(t => t.Id == id);
    } 
}