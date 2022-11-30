using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface ITicketManagementService
{
    Task<(bool isSucceed, IActionResult? actionResult, TicketDto ticket)> AddTicket(CreateTicketDto createTicketDto);
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> tickets,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetTickets(TicketParameters parameters);
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject ticket)> GetTicket(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, TicketDto ticket)> UpdateTicket(UpdateTicketDto updateTicketDto);
    Task<(bool isSucceed, IActionResult? actionResult)> DeleteTicket(int id);
    Task<bool> IsTicketExists(int id);
}