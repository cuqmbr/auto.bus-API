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

    [HttpPost]
    public async Task<IActionResult> AddVehicle(CreateVehicleDto vehicle)
    {
        var result = await _vehicleManagementService.AddVehicle(vehicle);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return CreatedAtAction(nameof(GetVehicle), new {id = result.vehicle.Id}, result.vehicle);
    }

    [HttpGet]
    public async Task<IActionResult> GetVehicles([FromQuery] VehicleParameters parameters)
    {
        var result = await _vehicleManagementService.GetVehicles(parameters);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.vehicles);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicle(int id, [FromQuery] string? fields)
    {
        if (!await _vehicleManagementService.IsVehicleExists(id))
        {
            return NotFound();
        }

        var result = await _vehicleManagementService.GetVehicle(id, fields);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }

        return Ok(result.vehicle);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(int id, UpdateVehicleDto vehicle)
    {
        if (id != vehicle.Id)
        {
            return BadRequest();
        }
        
        var result = await _vehicleManagementService.UpdateVehicle(vehicle);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return Ok(result.vehicle);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        if (!await _vehicleManagementService.IsVehicleExists(id))
        {
            return NotFound();
        }
        
        var result = await _vehicleManagementService.DeleteVehicle(id);
        
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return NoContent();
    }
}