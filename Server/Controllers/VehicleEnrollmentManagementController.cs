using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Controllers;


[Route("api/vehicleEnrollments")]
[ApiController]
public class VehicleEnrollmentManagementController : ControllerBase
{
    private readonly IVehicleEnrollmentManagementService _vehicleEnrollmentManagementService;
    
    public VehicleEnrollmentManagementController(IVehicleEnrollmentManagementService vehicleEnrollmentManagementService)
    {
        _vehicleEnrollmentManagementService = vehicleEnrollmentManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddEnrollment(CreateVehicleEnrollmentDto enrollment)
    {
        var result = await _vehicleEnrollmentManagementService.AddEnrollment(enrollment);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return CreatedAtAction(nameof(GetEnrollment), new {id = result.enrollment.Id}, result.enrollment);
    }

    [HttpGet]
    public async Task<IActionResult> GetEnrollments([FromQuery] VehicleEnrollmentParameters parameters)
    {
        var result = await _vehicleEnrollmentManagementService.GetEnrollments(parameters);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.enrollments);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEnrollment(int id, [FromQuery] string? fields)
    {
        if (!await _vehicleEnrollmentManagementService.IsEnrollmentExists(id))
        {
            return NotFound();
        }

        var result = await _vehicleEnrollmentManagementService.GetEnrollment(id, fields);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }

        return Ok(result.enrollment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(int id, UpdateVehicleEnrollmentDto enrollment)
    {
        if (id != enrollment.Id)
        {
            return BadRequest();
        }
        
        var result = await _vehicleEnrollmentManagementService.UpdateEnrollment(enrollment);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return Ok(result.enrollment);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEnrollment(int id)
    {
        if (!await _vehicleEnrollmentManagementService.IsEnrollmentExists(id))
        {
            return NotFound();
        }
        
        var result = await _vehicleEnrollmentManagementService.DeleteEnrollment(id);
        
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return NoContent();
    }
}