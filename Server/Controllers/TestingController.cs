using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;
    
[Route("api/[controller]")]
[ApiController]
public class TestingController : ControllerBase
{
    [HttpPost("timezone")]
    public async Task<IActionResult> SetTimeZone(string timeZone)
    {
        var cookieOptions = new CookieOptions()
        {
            Expires = DateTimeOffset.MaxValue
        };
        Response.Cookies.Append("timeZone", timeZone, cookieOptions);
        return Ok();
    }
    
    [HttpGet("timezone")]
    public async Task<IActionResult> GetTimeZone()
    {
        if (Request.Cookies.TryGetValue("timeZone", out string? tz))
        {
            return Ok(tz);
        }

        return NotFound();
    }

    [HttpPost("timespan")]
    public async Task<IActionResult> TestTimeSpan([FromBody] TimeSpan ts)
    {
        return Ok();
    }
}

