using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters.Objects;

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
    [HttpPost]
    public async Task<IActionResult> AddDriver(CreateDriverDto Driver)
    {
        var result = await _driverManagementService.AddDriver(Driver);
    
        if (!result.isSucceeded)
        {
            return result.actionResult;
        }
    
        return CreatedAtAction(nameof(GetDriver), new {id = result.driver.Id}, result.driver);
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
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDriver(string id, UpdateDriverDto driver)
    {
        var result = await _driverManagementService.UpdateDriver(id, driver);
    
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