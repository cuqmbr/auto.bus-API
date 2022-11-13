using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Controllers;

[Route("api/routeAddresses")]
[ApiController]
public class RouteAddressManagementController : ControllerBase
{
    private readonly IRouteAddressManagementService _routeAddressManagementService;
    
    public RouteAddressManagementController(IRouteAddressManagementService routeAddressManagementService)
    {
        _routeAddressManagementService = routeAddressManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddRouteAddress(CreateRouteAddressDto routeAddress)
    {
        var result = await _routeAddressManagementService.AddRouteAddress(routeAddress);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return CreatedAtAction(nameof(GetRouteAddress), new {id = result.routeAddress.Id}, result.routeAddress);
    }

    [HttpGet]
    public async Task<IActionResult> GetRouteAddresses([FromQuery] RouteAddressParameters parameters)
    {
        var result = await _routeAddressManagementService.GetRouteAddresses(parameters);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.routeAddresses);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRouteAddress(int id, [FromQuery] string? fields)
    {
        if (!await _routeAddressManagementService.IsRouteAddressExists(id))
        {
            return NotFound();
        }

        var result = await _routeAddressManagementService.GetRouteAddress(id, fields);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }

        return Ok(result.routeAddress);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRouteAddress(int id, UpdateRouteAddressDto routeAddress)
    {
        if (id != routeAddress.Id)
        {
            return BadRequest();
        }
        
        var result = await _routeAddressManagementService.UpdateRouteAddress(routeAddress);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return Ok(result.routeAddress);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRouteAddress(int id)
    {
        if (!await _routeAddressManagementService.IsRouteAddressExists(id))
        {
            return NotFound();
        }
        
        var result = await _routeAddressManagementService.DeleteRouteAddress(id);
        
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return NoContent();
    }
}