using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Controllers;

[Route("api/routes")]
[ApiController]
public class RouteManagementController : ControllerBase
{
    private readonly IRouteManagementService _routeManagementService;
    
    public RouteManagementController(IRouteManagementService routeManagementService)
    {
        _routeManagementService = routeManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddRoute(CreateRouteDto route)
    {
        var result = await _routeManagementService.AddRoute(route);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return CreatedAtAction(nameof(GetRoute), new {id = result.route.Id}, result.route);
    }

    [HttpGet]
    public async Task<IActionResult> GetRoutes([FromQuery] RouteParameters parameters)
    {
        var result = await _routeManagementService.GetRoutes(parameters);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.routes);
    }
    
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
            return BadRequest(result.message);
        }

        return Ok(result.route);
    }

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
            return BadRequest(result.message);
        }
    
        return Ok(result.route);
    }
    
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
            return BadRequest(result.message);
        }
    
        return NoContent();
    }
}