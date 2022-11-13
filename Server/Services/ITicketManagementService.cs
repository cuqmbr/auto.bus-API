using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Services;

public interface ITicketManagementService
{
    Task<(bool isSucceed, string message, TicketDto ticket)> AddTicket(CreateTicketDto createTicketDto);
    Task<(bool isSucceed, string message, IEnumerable<TicketDto> tickets,
        PagingMetadata<Ticket> pagingMetadata)> GetTickets(TicketParameters parameters);
    Task<(bool isSucceed, string message, TicketDto ticket)> GetTicket(int id, string? fields);
    Task<(bool isSucceed, string message, UpdateTicketDto ticket)> UpdateTicket(UpdateTicketDto updateTicketDto);
    Task<(bool isSucceed, string message)> DeleteTicket(int id);
    Task<bool> IsTicketExists(int id);
}