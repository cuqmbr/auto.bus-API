using System.Text.Json.Serialization;

namespace SharedModels.Responses;

public class AuthenticationResponse
{
    public string Message { get; set; } = null!;
    public bool IsAuthenticated { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> Roles { get; set; } = null!;
    public string Token { get; set; } = null!;
}