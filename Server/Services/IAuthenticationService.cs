using Microsoft.AspNetCore.Mvc;
using SharedModels.Requests;
using SharedModels.Responses;

namespace Server.Services;

public interface IAuthenticationService
{
    Task<(bool succeeded, IActionResult actionResult)> Register(RegistrationRequest request);
    
    Task<(bool succeeded, IActionResult actionResult)> ConfirmRegistrationEmail(ConfirmRegistrationEmailRequest request);
    
    Task<(bool succeeded, IActionResult actionResult)> ConfirmRegistrationPhoneNumber(ConfirmRegistrationPhoneNumberRequest numberRequest);

    Task<(bool succeeded, AuthenticationResponse authResponse, string? refreshToken)>
        AuthenticateAsync(AuthenticationRequest request);
    
    Task<(bool succeeded, AuthenticationResponse authResponse, string? refreshToken)>
        AuthenticateWithGoogleAsync(GoogleAuthenticationRequest request);

    Task<(bool succeeded, AuthenticationResponse authResponse, string? refreshToken)>
        RenewRefreshTokenAsync(string? token);

    Task<bool> RevokeRefreshToken(string? token);
}
