using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters.Objects;

namespace Server.Controllers;

[Route("api/routeAddresses")]
[ApiController]
public class RouteAddressController : ControllerBase
{
    private readonly IRouteAddressManagementService _routeAddressManagementService;
    
    public RouteAddressController(IRouteAddressManagementService routeAddressManagementService)
    {
        _routeAddressManagementService = routeAddressManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddRouteAddress(CreateRouteAddressDto routeAddress)
    {
        var result = await _routeAddressManagementService.AddRouteAddress(routeAddress);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return CreatedAtAction(nameof(GetRouteAddress), new {id = result.routeAddress.Id}, result.routeAddress);
    }

    [HttpGet]
    public async Task<IActionResult> GetRouteAddresses([FromQuery] RouteAddressParameters parameters)
    {
        var result = await _routeAddressManagementService.GetRouteAddresses(parameters);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.routeAddresses);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRouteAddress(int id, [FromQuery] string? fields)
    {
        var result = await _routeAddressManagementService.GetRouteAddress(id, fields);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.routeAddress);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRouteAddress(int id, UpdateRouteAddressDto routeAddress)
    {
        var result = await _routeAddressManagementService.UpdateRouteAddress(routeAddress);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return Ok(result.routeAddress);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRouteAddress(int id)
    {
        var result = await _routeAddressManagementService.DeleteRouteAddress(id);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return NoContent();
    }
}