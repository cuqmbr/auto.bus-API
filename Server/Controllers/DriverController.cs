using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters.Objects;
using SharedModels.Requests;

namespace Server.Controllers;

[Authorize]
[Route("api/drivers")]
[ApiController]
public class DriverController : ControllerBase
{
    private readonly IDriverManagementService _driverManagementService;

    public DriverController(IDriverManagementService driverManagementService)
    {
        _driverManagementService = driverManagementService;
    }
    
    [Authorize(Policy = "CompanyAccess")]
    [HttpPost("register")]
    public async Task<IActionResult> AddDriver(DriverRegistrationRequest driverRegistrationRequest)
    {
        var result = await _driverManagementService.RegisterDriver(driverRegistrationRequest);
    
        if (!result.isSucceeded)
        {
            return result.actionResult;
        }

        return Ok();
    }

    [Authorize(Policy = "CompanyAccess")]
    [HttpGet]
    public async Task<IActionResult> GetDrivers([FromQuery] CompanyDriverParameters parameters)
    {
        var result = await _driverManagementService.GetDrivers(parameters);

        if (!result.isSucceeded)
        {
            return result.actionResult;
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.drivers);
    }
    
    [Authorize(Policy = "CompanyAccess")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDriver(string id, [FromQuery] string? fields)
    {
        var result = await _driverManagementService.GetDriver(id, fields);

        if (!result.isSucceeded)
        {
            return result.actionResult;
        }

        return Ok(result.driver);
    }

    [Authorize(Policy = "CompanyAccess")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDriver(string id)
    {
        var result = await _driverManagementService.DeleteDriver(id);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return NoContent();
    }
}