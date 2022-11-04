using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Controllers;

[Route("api/states")] 
[ApiController] 
public class StateManagementController : ControllerBase 
{
    private readonly IStateManagementService _stateManagementService;
    
    public StateManagementController(IStateManagementService stateManagementService)
    {
        _stateManagementService = stateManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddState(CreateStateDto state)
    {
        var result = await _stateManagementService.AddState(state);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return CreatedAtAction(nameof(GetState), new {id = result.state.Id}, result.state);
    }

    [HttpGet]
    public async Task<IActionResult> GetStates([FromQuery] StateParameters parameters)
    {
        var result = await _stateManagementService.GetStates(parameters);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.states);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetState(int id, [FromQuery] string? fields)
    {
        if (!await _stateManagementService.IsStateExists(id))
        {
            return NotFound();
        }

        var result = await _stateManagementService.GetState(id, fields);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }

        return Ok(result.state);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoute(int id, UpdateStateDto state)
    {
        if (id != state.Id)
        {
            return BadRequest();
        }
        
        var result = await _stateManagementService.UpdateState(state);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return Ok(result.state);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoute(int id)
    {
        if (!await _stateManagementService.IsStateExists(id))
        {
            return NotFound();
        }
        
        var result = await _stateManagementService.DeleteState(id);
        
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return NoContent();
    }
}

