using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AutomationController : ControllerBase
{
    private readonly AutomationService _automationService;

    public AutomationController(AutomationService automationService)
    {
        _automationService = automationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoute(int from, int to, DateTime date)
    {
        var result = await _automationService.GetRoute(from, to, date);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.result);
    }
}

