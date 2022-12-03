using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters.Objects;

namespace Server.Controllers;


[Route("api/vehicleEnrollments")]
[ApiController]
public class VehicleEnrollmentController : ControllerBase
{
    private readonly IVehicleEnrollmentManagementService _vehicleEnrollmentManagementService;
    
    public VehicleEnrollmentController(IVehicleEnrollmentManagementService vehicleEnrollmentManagementService)
    {
        _vehicleEnrollmentManagementService = vehicleEnrollmentManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddEnrollment(CreateVehicleEnrollmentDto enrollment)
    {
        var result = await _vehicleEnrollmentManagementService.AddEnrollment(enrollment);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return CreatedAtAction(nameof(GetEnrollment), new {id = result.enrollment.Id}, result.enrollment);
    }
    
    [HttpPost("withDetails")]
    public async Task<IActionResult> AddEnrollmentWithDetails(CreateVehicleEnrollmentWithDetailsDto enrollment)
    {
        var result = await _vehicleEnrollmentManagementService.AddEnrollmentWithDetails(enrollment);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return CreatedAtAction(nameof(GetEnrollment), new {id = result.enrollment.Id}, result.enrollment);
    }

    [HttpGet]
    public async Task<IActionResult> GetEnrollments([FromQuery] VehicleEnrollmentParameters parameters)
    {
        var result = await _vehicleEnrollmentManagementService.GetEnrollments(parameters);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.enrollments);
    }
    
    [HttpGet("withDetails")]
    public async Task<IActionResult> GetEnrollments([FromQuery] VehicleEnrollmentWithDetailsParameters parameters)
    {
        var result = await _vehicleEnrollmentManagementService.GetEnrollmentsWithDetails(parameters);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.enrollments);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEnrollment(int id, [FromQuery] string? fields)
    {
        var result = await _vehicleEnrollmentManagementService.GetEnrollment(id, fields);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.enrollment);
    }
    
    [HttpGet("withDetails/{id}")]
    public async Task<IActionResult> GetEnrollmentWithDetails(int id, [FromQuery] string? fields)
    {
        var result = await _vehicleEnrollmentManagementService.GetEnrollmentWithDetails(id, fields);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.enrollment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(int id, UpdateVehicleEnrollmentDto enrollment)
    {
        var result = await _vehicleEnrollmentManagementService.UpdateEnrollment(enrollment);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return Ok(result.enrollment);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEnrollment(int id)
    {
        var result = await _vehicleEnrollmentManagementService.DeleteEnrollment(id);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return NoContent();
    }
}