using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters.Objects;

namespace Server.Controllers;

[Authorize]
[Route("api/states")] 
[ApiController] 
public class StateController : ControllerBase 
{
    private readonly IStateManagementService _stateManagementService;
    
    public StateController(IStateManagementService stateManagementService)
    {
        _stateManagementService = stateManagementService;
    }

    [Authorize(Policy = "AdministratorAccess")]
    [HttpPost]
    public async Task<IActionResult> AddState(CreateStateDto state)
    {
        var result = await _stateManagementService.AddState(state);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return CreatedAtAction(nameof(GetState), new {id = result.state.Id}, result.state);
    }

    [Authorize(Policy = "CompanyAccess")]
    [HttpGet]
    public async Task<IActionResult> GetStates([FromQuery] StateParameters parameters)
    {
        var result = await _stateManagementService.GetStates(parameters);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.states);
    }
    
    [Authorize(Policy = "CompanyAccess")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetState(int id, [FromQuery] string? fields)
    {
        var result = await _stateManagementService.GetState(id, fields);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.state);
    }

    [Authorize(Policy = "AdministratorAccess")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateState(int id, UpdateStateDto state)
    {
        if (id != state.Id)
        {
            return BadRequest();
        }
        
        var result = await _stateManagementService.UpdateState(state);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return Ok(result.state);
    }
    
    [Authorize(Policy = "AdministratorAccess")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteState(int id)
    {
        var result = await _stateManagementService.DeleteState(id);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return NoContent();
    }
}

