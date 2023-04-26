using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Server.Configurations;
using Server.Services;
using SharedModels.Requests;
using SharedModels.Responses;

namespace Server.Controllers;

[Route("api/authentication")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly Jwt _jwt;

    public AuthenticationController(IAuthenticationService authService, IOptions<Jwt> jwt)
    {
        _authService = authService;
        _jwt = jwt.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegistrationRequest registerRequest)
    {
        var (succeeded, message) = await _authService.RegisterAsync(registerRequest);

        if (!succeeded)
        {
            return BadRequest(new ResponseBase {Message = message});
        }
        
        return Ok(new ResponseBase{ Message = message });
    }

    [HttpGet("confirmEmail")]
    public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string email, [FromQuery] string token,
        [FromQuery] string redirectionUrl)
    {
        var (succeeded, message) = await _authService.ConfirmEmailAsync(email, token);
        
        if (!succeeded)
        {
            return BadRequest(new ResponseBase {Message = message});
        }
        
        return Redirect(redirectionUrl);
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> GetTokenAsync(AuthenticationRequest authRequest)
    {
        var (succeeded, authResponse, refreshToken) =
            await _authService.AuthenticateAsync(authRequest);

        if (!succeeded)
        {
            return BadRequest(authResponse);
        }

        SetRefreshTokenInCookie(refreshToken!);
        
        return Ok(authResponse);
    }

    [HttpPost("renew-session")]
    public async Task<IActionResult> RenewTokens()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        var (succeeded, authResponse, newRefreshToken) =
            await _authService.RenewRefreshTokenAsync(refreshToken);

        if (!succeeded)
        {
            return BadRequest(authResponse);
        }
        
        if (!String.IsNullOrEmpty(newRefreshToken))
        {
            SetRefreshTokenInCookie(newRefreshToken);
        }

        return Ok(authResponse);
    }
    
    [Authorize]
    [HttpPost("revoke-session")]
    public async Task<IActionResult> RevokeToken()
    {
        // accept token from request body or cookie
        var token = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new ResponseBase{ Message = "Refresh token is required." });
        }
        
        var response = await _authService.RevokeRefreshToken(token);
        if (!response)
        {
            return NotFound(new ResponseBase{ Message = "Refresh token not found." });
        }
        
        Response.Cookies.Delete("refreshToken");
        
        return Ok(new ResponseBase{ Message = "Refresh token revoked." });
    }
    
    private void SetRefreshTokenInCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(_jwt.RefreshTokenValidityInDays)
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}