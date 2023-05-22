using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters.Objects;

namespace Server.Controllers;

[Authorize]
[Route("api/routes")]
[ApiController]
public class RouteController : ControllerBase
{
    private readonly IRouteManagementService _routeManagementService;
    
    public RouteController(IRouteManagementService routeManagementService)
    {
        _routeManagementService = routeManagementService;
    }

    [Authorize(Policy = "CompanyAccess")]
    [HttpPost]
    public async Task<IActionResult> AddRoute(CreateRouteDto route)
    {
        var result = await _routeManagementService.AddRoute(route);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return CreatedAtAction(nameof(GetRoute), new {id = result.route.Id}, result.route);
    }

    [Authorize(Policy = "CompanyAccess")]
    [HttpPost("withAddresses")]
    public async Task<IActionResult> AddRouteWithAddresses(CreateRouteWithAddressesDto route)
    {
        var result = await _routeManagementService.AddRouteWithAddresses(route);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return CreatedAtAction(nameof(GetRoute), new {id = result.route.Id}, result.route);
    }

    [Authorize(Policy = "DriverAccess")]
    [HttpGet]
    public async Task<IActionResult> GetRoutes([FromQuery] RouteParameters parameters)
    {
        var result = await _routeManagementService.GetRoutes(parameters);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.routes);
    }
    
    [Authorize(Policy = "DriverAccess")]
    [HttpGet("withAddresses")]
    public async Task<IActionResult> GetRouteWithAddresses([FromQuery] RouteWithAddressesParameters parameters)
    {
        var result = await _routeManagementService.GetRoutesWithAddresses(parameters);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.routes);
    }
    
    [Authorize(Policy = "DriverAccess")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoute(int id, [FromQuery] string? fields)
    {
        if (!await _routeManagementService.IsRouteExists(id))
        {
            return NotFound();
        }

        var result = await _routeManagementService.GetRoute(id, fields);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.route);
    }
    
    [Authorize(Policy = "DriverAccess")]
    [HttpGet("withAddresses/{id}")]
    public async Task<IActionResult> GetRouteWithAddresses(int id, [FromQuery] string? fields)
    {
        if (!await _routeManagementService.IsRouteExists(id))
        {
            return NotFound();
        }

        var result = await _routeManagementService.GetRouteWithAddresses(id, fields);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.route);
    }

    [Authorize(Policy = "CompanyAccess")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoute(int id, UpdateRouteDto route)
    {
        if (id != route.Id)
        {
            return BadRequest();
        }
        
        var result = await _routeManagementService.UpdateRoute(route);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return Ok(result.route);
    }
    
    [Authorize(Policy = "CompanyAccess")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoute(int id)
    {
        if (!await _routeManagementService.IsRouteExists(id))
        {
            return NotFound();
        }
        
        var result = await _routeManagementService.DeleteRoute(id);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return NoContent();
    }
}