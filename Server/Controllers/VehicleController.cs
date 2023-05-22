using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters.Objects;

namespace Server.Controllers;

[Route("api/vehicles")]
[ApiController]
public class VehicleController : ControllerBase
{
    private readonly IVehicleManagementService _vehicleManagementService;
    
    public VehicleController(IVehicleManagementService vehicleManagementService)
    {
        _vehicleManagementService = vehicleManagementService;
    }

    [Authorize(Policy = "CompanyAccess")]
    [HttpPost]
    public async Task<IActionResult> AddVehicle(CreateVehicleDto vehicle)
    {
        var result = await _vehicleManagementService.AddVehicle(vehicle);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return CreatedAtAction(nameof(GetVehicle), new {id = result.vehicle.Id}, result.vehicle);
    }

    [Authorize(Policy = "CompanyAccess")]
    [HttpGet]
    public async Task<IActionResult> GetVehicles([FromQuery] VehicleParameters parameters)
    {
        var result = await _vehicleManagementService.GetVehicles(parameters);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.vehicles);
    }
    
    [Authorize(Policy = "DriverAccess")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicle(int id, [FromQuery] string? fields)
    {
        var result = await _vehicleManagementService.GetVehicle(id, fields);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.vehicle);
    }

    [Authorize(Policy = "CompanyAccess")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(int id, UpdateVehicleDto vehicle)
    {
        var result = await _vehicleManagementService.UpdateVehicle(vehicle);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return Ok(result.vehicle);
    }
    
    [Authorize(Policy = "CompanyAccess")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        var result = await _vehicleManagementService.DeleteVehicle(id);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return NoContent();
    }
}