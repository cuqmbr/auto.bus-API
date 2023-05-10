using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[Route("api/search")]
[ApiController]
public class VehicleEnrollmentSearchController : ControllerBase
{
    private readonly VehicleEnrollmentSearchService _vehicleEnrollmentSearchService;

    public VehicleEnrollmentSearchController(VehicleEnrollmentSearchService vehicleEnrollmentSearchService)
    {
        _vehicleEnrollmentSearchService = vehicleEnrollmentSearchService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoute(int from, int to, DateTime date)
    {
        var result = await _vehicleEnrollmentSearchService.GetRoute(from, to, date);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.result);
    }
}

