using Microsoft.AspNetCore.Mvc;
using Server.Services;
using SharedModels.Requests;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthenticationController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegistrationRequest model)
    {
        var result = await _authService.RegisterAsync(model);

        if (!result.succeeded)
        {
            return BadRequest(result.message);
        }
        
        return Ok(result);
    }

    [HttpPost("token")]
    public async Task<IActionResult> GetTokenAsync(AuthenticationRequest authRequest)
    {
        var authResponse = await _authService.GetTokenAsync(authRequest);

        if (!authResponse.IsAuthenticated)
        {
            return BadRequest(authResponse);
        }

        return Ok(authResponse);
    }
}