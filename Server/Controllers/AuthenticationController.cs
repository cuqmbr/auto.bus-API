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
    public async Task<IActionResult> RegisterAsync([FromBody] RegistrationRequest request)
    {
        var result = await _authService.Register(request);

        if (!result.succeeded)
        {
            return result.actionResult;
        }
        
        return Ok();
    }

    [HttpPost("confirmEmail")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmRegistrationEmailRequest request)
    {
        var result = await _authService.ConfirmRegistrationEmail(request);
        
        if (!result.succeeded)
        {
            return result.actionResult;
        }

        return Ok();
    }
    
    [HttpPost("confirmPhoneNumber")]
    public async Task<IActionResult> ConfirmPhoneNumber([FromBody] ConfirmRegistrationPhoneNumberRequest request)
    {
        var result = await _authService.ConfirmRegistrationPhoneNumber(request);
        
        if (!result.succeeded)
        {
            return result.actionResult;
        }

        return Ok();
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate(AuthenticationRequest authRequest)
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

    [HttpPost("googleoauth")]
    public async Task<IActionResult> AuthenticateWithGoogle(GoogleAuthenticationRequest authRequest)
    {
        var (succeeded, authResponse, refreshToken) =
            await _authService.AuthenticateWithGoogleAsync(authRequest);

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