using Server.Models;
using SharedModels.Requests;
using SharedModels.Responses;

namespace Server.Services;

public interface IAuthenticationService
{
    Task<(bool succeeded, string message)> RegisterAsync(RegistrationRequest regRequest);
    
    Task<AuthenticationResponse> GetTokenAsync(AuthenticationRequest authRequest);
}