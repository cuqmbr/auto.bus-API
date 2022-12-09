using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface ITicketGroupManagementService
{
    Task<(bool isSucceed, IActionResult? actionResult, TicketGroupDto ticketGroup)> AddTicketGroup(CreateTicketGroupDto createTicketGroupDto);
    Task<(bool isSucceed, IActionResult? actionResult, TicketGroupWithTicketsDto ticketGroup)> AddTicketGroupWithTickets(CreateTicketGroupWithTicketsDto createTicketGroupWithTicketsDto);
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> ticketGroups,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetTicketGroups(TicketGroupParameters parameters); 
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> ticketGroups,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetTicketGroupsWithTickets(TicketGroupWithTicketsParameters parameters); 
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject ticketGroup)> GetTicketGroup(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject ticketGroup)> GetTicketGroupWithTickets(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, UpdateTicketGroupDto ticketGroup)> UpdateTicketGroup(UpdateTicketGroupDto updateTicketGroupDto);
    Task<(bool isSucceed, IActionResult? actionResult)> DeleteTicketGroup(int id);
    Task<bool> IsTicketGroupExist(int id);
}