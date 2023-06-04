using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Server.Configurations;
using Server.Constants;
using Server.Models;
using SharedModels.Requests.Authentication;
using SharedModels.Responses;
using Utils;

namespace Server.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly Jwt _jwt;
    
    private readonly UserManager<User> _userManager;
    private readonly IEmailSenderService _emailSender;
    private readonly IConfiguration _configuration;
    
    public AuthenticationService(UserManager<User> userManager, IOptions<Jwt> jwt,
        IEmailSenderService emailSender, IConfiguration configuration)
    {
        _jwt = jwt.Value;
        
        _userManager = userManager;
        _emailSender = emailSender;
        _configuration = configuration;

        _userManager.UserValidators.Clear();
    }

    public async Task<(bool succeeded, IActionResult actionResult)> Register(RegistrationRequest request)
    {
        var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
        if (userWithSameEmail != null)
        {
            return (false, new BadRequestObjectResult("Email is already registered."));
        }

        var userWithSamePhone = await _userManager.Users
            .SingleOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (userWithSamePhone != null)
        {
            return (false, new BadRequestObjectResult("Phone is already registered."));
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Patronymic = request.Patronymic,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        var createUserResult = await _userManager.CreateAsync(user, request.Password);
        if (!createUserResult.Succeeded)
        {
            return (false, new BadRequestObjectResult(createUserResult.Errors));
        }

        await _userManager.AddToRoleAsync(user, Identity.DefaultRole.ToString());

        var emailMessage =
            "Someone registered an account on our service with your email.\n" +
            "If this was not you, please ignore this message.";
        
        try { await _emailSender.SendMail(user.Email, "Account Registration", emailMessage); }
        catch (Exception e) { /* ignored */ }
        
        return (true, null!);
    }

    public async Task<(bool succeeded, IActionResult actionResult)> SendEmailConfirmationCode(SendConfirmationRegistrationEmailRequest request)
    {
        var dbUser = await _userManager.FindByEmailAsync(request.Email);

        if (dbUser == null)
        {
            return (false, new BadRequestObjectResult("Email not registered"));
        }
        
        var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(dbUser);
        var confirmationMessage = $"Confirmation code: {emailConfirmationToken}";

        try { await _emailSender.SendMail(dbUser.Email, "Email Confirmation", confirmationMessage); }
        catch (Exception e) { /* ignored */ }

        return (true, null!);
    }

    public async Task<(bool succeeded, IActionResult actionResult)> ConfirmRegistrationEmail(ConfirmRegistrationEmailRequest request)
    {
        var dbUser = await _userManager.FindByEmailAsync(request.Email);

        var result = await _userManager.ConfirmEmailAsync(dbUser, request.Token);
        if (!result.Succeeded)
        {
            return (false, new BadRequestObjectResult($"Error confirming email."));
        }

        return (true, null!);
    }

    public Task<(bool succeeded, IActionResult actionResult)> SendPhoneNumberConfirmationCode(SendConfirmationRegistrationPhoneNumberRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<(bool succeeded, IActionResult actionResult)> ConfirmRegistrationPhoneNumber(ConfirmRegistrationPhoneNumberRequest numberRequest)
    {
        var dbUser = await _userManager.Users.FirstAsync(u => u.PhoneNumber == numberRequest.PhoneNumber);

        // TODO: Add phone number confirmation token validation

        dbUser.PhoneNumberConfirmed = true;
        await _userManager.UpdateAsync(dbUser);
        
        return (true, null!);
    }

    public async Task<(bool succeeded, AuthenticationResponse authResponse, string? refreshToken)>
        Authenticate(AuthenticationRequest request)
    {
        var authResponse = new AuthenticationResponse();

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            authResponse.Message = $"No accounts registered with {request.Email}.";
            return (false, authResponse, null);
        }
        
        if (!user.EmailConfirmed)
        {
            authResponse.Message = "You must confirm your email before logging in";
            return (false, authResponse, null);
        }
        
        if (!user.PhoneNumberConfirmed)
        {
            authResponse.Message = "You must confirm your phone number before logging in";
            return (false, authResponse, null);
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            authResponse.Message = "Incorrect email or password.";
            return (false, authResponse, null);
        }

        var jwtSecurityToken = await CreateJwtToken(user);
        authResponse.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        string refreshTokenString;
        
        if (user.RefreshTokens.Any(t => t.IsActive))
        {
            var activeRefreshToken =
                user.RefreshTokens.First(t => t.IsActive);
            refreshTokenString = activeRefreshToken.Token;
            authResponse.RefreshTokenExpirationDate = activeRefreshToken.ExpiryDateTime;
        }
        else
        {
            var refreshToken = CreateRefreshToken();
            refreshTokenString = refreshToken.Token;
            authResponse.RefreshTokenExpirationDate = refreshToken.ExpiryDateTime;
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);
        }
        
        return (true, authResponse, refreshTokenString);
    }

    public async Task<(bool succeeded, AuthenticationResponse authResponse, string? refreshToken)>
        AuthenticateWithGoogle(GoogleAuthenticationRequest request)
    {
        GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings();

        settings.Audience = new List<string> { _configuration["Authentication:Google:ClientId"] };
            
        GoogleJsonWebSignature.Payload payload  = GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings).Result;

        var authResponse = new AuthenticationResponse();

        var user = await _userManager.FindByEmailAsync(payload.Email);
        if (user == null)
        {
            var createUserResult = await _userManager.CreateAsync(new User
            {
                Email = payload.Email,
                EmailConfirmed = payload.EmailVerified,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName
            });
            
            if (!createUserResult.Succeeded)
            {
                authResponse.Message = $"{createUserResult.Errors?.First().Description}";
                return (false, authResponse, null);
            }
        }
        
        string refreshTokenString;
        
        if (user.RefreshTokens.Any(t => t.IsActive))
        {
            var activeRefreshToken =
                user.RefreshTokens.First(t => t.IsActive);
            refreshTokenString = activeRefreshToken.Token;
            authResponse.RefreshTokenExpirationDate = activeRefreshToken.ExpiryDateTime;
        }
        else
        {
            var refreshToken = CreateRefreshToken();
            refreshTokenString = refreshToken.Token;
            authResponse.RefreshTokenExpirationDate = refreshToken.ExpiryDateTime;
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);
        }
        
        return (true, authResponse, refreshTokenString);
    }

    public async Task<(bool succeeded, AuthenticationResponse authResponse, string? refreshToken)>
        RenewRefreshToken(string? token)
    {
        var authResponse = new AuthenticationResponse();

        var user = await _userManager.Users.SingleOrDefaultAsync(u =>
            u.RefreshTokens.Any(t => t.Token == token));

        if (user == null)
        {
            authResponse.Message = "Refresh token did not mach any user.";
            return (false, authResponse, null);
        }

        var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

        if (!refreshToken.IsActive)
        {
            authResponse.Message = "Refresh token expired.";
            return (false, authResponse, null);
        }
        
        //Revoke Current Refresh Token
        refreshToken.Revoked = DateTime.UtcNow;
        
        //Generate new Refresh Token and save to Database
        var newRefreshToken = CreateRefreshToken();
        user.RefreshTokens.Add(newRefreshToken);
        await _userManager.UpdateAsync(user);
        
        //Generates new jwt
        var jwtSecurityToken = await CreateJwtToken(user);
        authResponse.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        
        authResponse.RefreshTokenExpirationDate = newRefreshToken.ExpiryDateTime;
        
        return (true, authResponse, newRefreshToken.Token);
    }

    public async Task<bool> RevokeRefreshToken(string? token)
    {
        var user = await _userManager.Users.SingleOrDefaultAsync(u =>
            u.RefreshTokens.Any(t => t.Token == token));
        
        if (user == null)
        {
            return false;
        }

        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
        {
            return false;
        }
        
        refreshToken.Revoked = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        
        return true;
    }
    
    private async Task<JwtSecurityToken> CreateJwtToken(User user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        
        var roleClaims = new List<Claim>();
        
        foreach (var role in roles)
        {
            roleClaims.Add(new Claim("roles", role));
        }
        
        var claims = new[]
            {
                new Claim(JwtStandardClaimNames.Sub, user.Id),
                new Claim(JwtStandardClaimNames.Name, user.GetFullName()),
                new Claim(JwtStandardClaimNames.GivenName, user.FirstName),
                new Claim(JwtStandardClaimNames.FamilyName, user.LastName),
                new Claim(JwtStandardClaimNames.MiddleName, user.Patronymic),
                new Claim(JwtStandardClaimNames.Gender, user.Gender?.ToString() ?? "Undefined"),
                new Claim(JwtStandardClaimNames.BirthDate, user.BirthDate?.ToString()?? "Undefined"),
                new Claim(JwtStandardClaimNames.Email, user.Email),
                new Claim(JwtStandardClaimNames.EmailVerified, user.EmailConfirmed.ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, DateTime.UtcNow.AddMinutes(_jwt.ValidityInMinutes).ToString(CultureInfo.InvariantCulture))
            }
            .Union(userClaims)
            .Union(roleClaims);
        
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        
        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ValidityInMinutes),
            signingCredentials: signingCredentials);
        
        return jwtSecurityToken;
    }

    private RefreshToken CreateRefreshToken()
    {
        var randomNumber = new byte[32];

        using var rng = RandomNumberGenerator.Create();
        rng.GetNonZeroBytes(randomNumber);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            CreationDateTime = DateTime.UtcNow,
            ExpiryDateTime = DateTime.UtcNow.AddDays(_jwt.RefreshTokenValidityInDays)
        };
    }
}
