using Microsoft.AspNetCore.Mvc;
using SharedModels.Requests.Authentication;
using SharedModels.Responses;

namespace Server.Services;

public interface IAuthenticationService
{
    Task<(bool succeeded, IActionResult actionResult)> Register(RegistrationRequest request);
    
    Task<(bool succeeded, IActionResult actionResult)> SendEmailConfirmationCode(SendConfirmationRegistrationEmailRequest request);
    
    Task<(bool succeeded, IActionResult actionResult)> ConfirmRegistrationEmail(ConfirmRegistrationEmailRequest request);
    
    Task<(bool succeeded, IActionResult actionResult)> SendPhoneNumberConfirmationCode(SendConfirmationRegistrationPhoneNumberRequest request);
    
    Task<(bool succeeded, IActionResult actionResult)> ConfirmRegistrationPhoneNumber(ConfirmRegistrationPhoneNumberRequest numberRequest);

    Task<(bool succeeded, AuthenticationResponse authResponse, string? refreshToken)>
        Authenticate(AuthenticationRequest request);
    
    Task<(bool succeeded, AuthenticationResponse authResponse, string? refreshToken)>
        AuthenticateWithGoogle(GoogleAuthenticationRequest request);

    Task<(bool succeeded, AuthenticationResponse authResponse, string? refreshToken)>
        RenewRefreshToken(string? token);

    Task<bool> RevokeRefreshToken(string? token);
}
