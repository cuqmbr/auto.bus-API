using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface ITicketGroupManagementService
{
    Task<(bool isSucceed, IActionResult actionResult, IEnumerable<ExpandoObject> ticketGroups, PagingMetadata<ExpandoObject> pagingMetadata)>
        GetTicketGroups(TicketGroupParameters parameters); 
    
    Task<(bool isSucceed, IActionResult actionResult, ExpandoObject ticketGroup)>
        GetTicketGroup(int id, string? fields);
}