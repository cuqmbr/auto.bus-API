using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Server.Services;
using Server.Settings;
using SharedModels.Requests;

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
    public async Task<IActionResult> RegisterAsync([FromBody] RegistrationRequest model)
    {
        var result = await _authService.RegisterAsync(model);

        if (!result.succeeded)
        {
            return BadRequest(result.message);
        }
        
        return Ok(result);
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> GetTokenAsync(AuthenticationRequest authRequest)
    {
        var authResponse = await _authService.AuthenticateAsync(authRequest);

        if (!authResponse.IsAuthenticated)
        {
            return BadRequest(authResponse);
        }

        SetRefreshTokenInCookie(authResponse.RefreshToken);
        
        return Ok(authResponse);
    }

    [HttpPost("renew-session")]
    public async Task<IActionResult> RenewTokens()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        var authResponse = await _authService.RenewRefreshTokenAsync(refreshToken);

        if (!authResponse.IsAuthenticated)
        {
            return BadRequest(authResponse);
        }
        
        if (!String.IsNullOrEmpty(authResponse.RefreshToken))
        {
            SetRefreshTokenInCookie(authResponse.RefreshToken);
        }

        return Ok(authResponse);
    }
    
    [HttpPost("revoke-session")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeRefreshTokenRequest revokeRequest)
    {
        // accept token from request body or cookie
        var token = revokeRequest?.Token ?? Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Refresh token is required." });
        }
        
        var response = await _authService.RevokeRefreshToken(token);
        if (!response)
        {
            return NotFound(new { message = "Refresh token not found." });
        }
        
        return Ok(new { message = "Refresh token revoked." });
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