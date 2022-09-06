namespace SharedModels.Responses;

public class AuthenticationResponse : ResponseBase
{
    public string? Token { get; set; }
    public DateTime? RefreshTokenExpirationDate { get; set; }
}