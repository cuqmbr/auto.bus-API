using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Controllers;

[Route("api/tickets")]
[ApiController]
public class TicketManagementController : ControllerBase
{
    private readonly ITicketManagementService _ticketManagementService;
    
    public TicketManagementController(ITicketManagementService ticketManagementService)
    {
        _ticketManagementService = ticketManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddTicket(CreateTicketDto ticket)
    {
        var result = await _ticketManagementService.AddTicket(ticket);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return CreatedAtAction(nameof(GetTicket), new {id = result.ticket.Id}, result.ticket);
    }

    [HttpGet]
    public async Task<IActionResult> GetTickets([FromQuery] TicketParameters parameters)
    {
        var result = await _ticketManagementService.GetTickets(parameters);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.tickets);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicket(int id, [FromQuery] string? fields)
    {
        if (!await _ticketManagementService.IsTicketExists(id))
        {
            return NotFound();
        }

        var result = await _ticketManagementService.GetTicket(id, fields);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }

        return Ok(result.ticket);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTicket(int id, UpdateTicketDto ticket)
    {
        if (id != ticket.Id)
        {
            return BadRequest();
        }
        
        var result = await _ticketManagementService.UpdateTicket(ticket);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return Ok(result.ticket);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        if (!await _ticketManagementService.IsTicketExists(id))
        {
            return NotFound();
        }
        
        var result = await _ticketManagementService.DeleteTicket(id);
        
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return NoContent();
    }
}