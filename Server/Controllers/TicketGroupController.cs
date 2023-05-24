using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.QueryParameters.Objects;

namespace Server.Controllers;

[Authorize]
[Route("api/ticketGroups")]
[ApiController]
public class TicketGroupController : ControllerBase
{
    private readonly ITicketGroupManagementService _ticketGroupManagementService;
    
    public TicketGroupController(ITicketGroupManagementService ticketGroupManagementService)
    {
        _ticketGroupManagementService = ticketGroupManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTicketGroups([FromQuery] TicketGroupParameters parameters)
    {
        var result = await _ticketGroupManagementService.GetTicketGroups(parameters);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.ticketGroups);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicketGroup(int id, [FromQuery] string? fields)
    {
        var result = await _ticketGroupManagementService.GetTicketGroup(id, fields);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.ticketGroup);
    }
}