using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters.Objects;

namespace Server.Controllers;

[Route("api/ticketGroups")]
[ApiController]
public class TicketGroupController : ControllerBase
{
    private readonly ITicketGroupManagementService _ticketGroupManagementService;
    
    public TicketGroupController(ITicketGroupManagementService ticketGroupManagementService)
    {
        _ticketGroupManagementService = ticketGroupManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddTicketGroup(CreateTicketGroupDto ticketGroup)
    {
        var result = await _ticketGroupManagementService.AddTicketGroup(ticketGroup);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return CreatedAtAction(nameof(GetTicketGroup), new {id = result.ticketGroup.Id}, result.ticketGroup);
    }

    [HttpPost("withTickets")]
    public async Task<IActionResult> AddTicketGroupWithTickets(CreateTicketGroupWithTicketsDto ticketGroup)
    {
        var result = await _ticketGroupManagementService.AddTicketGroupWithTickets(ticketGroup);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return CreatedAtAction(nameof(GetTicketGroup), new {id = result.ticketGroup.Id}, result.ticketGroup);
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
    
    [HttpGet("withTickets")]
    public async Task<IActionResult> GetTicketGroupsWithTickets([FromQuery] TicketGroupWithTicketsParameters parameters)
    {
        var result = await _ticketGroupManagementService.GetTicketGroupsWithTickets(parameters);

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
    
    [HttpGet("withTickets/{id}")]
    public async Task<IActionResult> GetTicketGroupWithTickets(int id, [FromQuery] string? fields)
    {
        var result = await _ticketGroupManagementService.GetTicketGroupWithTickets(id, fields);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.ticketGroup);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTicketGroup(int id, UpdateTicketGroupDto ticketGroup)
    {
        if (id != ticketGroup.Id)
        {
            return BadRequest();
        }
        
        var result = await _ticketGroupManagementService.UpdateTicketGroup(ticketGroup);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return Ok(result.ticketGroup);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicketGroup(int id)
    {
        var result = await _ticketGroupManagementService.DeleteTicketGroup(id);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return NoContent();
    }
}

