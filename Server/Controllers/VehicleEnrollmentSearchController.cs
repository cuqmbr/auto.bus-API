using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[Route("api/")]
[ApiController]
public class VehicleEnrollmentSearchController : ControllerBase
{
    private readonly VehicleEnrollmentSearchService _vehicleEnrollmentSearchService;

    public VehicleEnrollmentSearchController(VehicleEnrollmentSearchService vehicleEnrollmentSearchService)
    {
        _vehicleEnrollmentSearchService = vehicleEnrollmentSearchService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetEnrollments(int fromCityId, int toCityId, DateTime date)
    {
        var result = await _vehicleEnrollmentSearchService.GetEnrollments(fromCityId, toCityId, date);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.result);
    }

    [HttpGet("autocomplete")]
    public async Task<IActionResult> AutocompleteCityName([FromQuery] string type, [FromQuery] string query, [FromQuery] int limit)
    {
        var result = await _vehicleEnrollmentSearchService.GetPopularCityNames(type, query, limit);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }
        
        return new OkObjectResult(result.cities);
    }
    
    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularCityNames([FromQuery] string type, [FromQuery] int limit)
    {
        var result = await _vehicleEnrollmentSearchService.GetPopularCityNames(type, limit);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }
        
        return new OkObjectResult(result.cities);
    }
}