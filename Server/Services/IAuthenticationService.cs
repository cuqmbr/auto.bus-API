using SharedModels.Requests;
using SharedModels.Responses;

namespace Server.Services;

public interface IAuthenticationService
{
    Task<(bool succeeded, string message)> RegisterAsync(RegistrationRequest regRequest);

    Task<(bool succeeded, string message)> ConfirmEmailAsync(string email, string token);
    
    Task<(bool succeeded, AuthenticationResponse authResponse, string? refreshToken)>
        AuthenticateAsync(AuthenticationRequest authRequest);
    
    Task<(bool succeeded, AuthenticationResponse authResponse, string? refreshToken)>
        AuthenticateWithGoogleAsync(GoogleAuthenticationRequest authRequest);

    Task<(bool succeeded, AuthenticationResponse authResponse, string? refreshToken)>
        RenewRefreshTokenAsync(string? token);

    Task<bool> RevokeRefreshToken(string? token);
}
