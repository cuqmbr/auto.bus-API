using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Server.Configurations;
using Server.Models;
using SharedModels.Requests;
using SharedModels.Responses;

namespace Server.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly Jwt _jwt;
    
    public AuthenticationService(UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager, IOptions<Jwt> jwt)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwt = jwt.Value;
    }

    public async Task<(bool succeeded, string message)> RegisterAsync(RegistrationRequest regRequest)
    {
        _userManager.UserValidators.Clear();
        
        var userWithSameEmail = await _userManager.FindByEmailAsync(regRequest.Email);
        if (userWithSameEmail != null)
        {
            return (false, $"Email is already registered.");
        }

        var userWithSamePhone = await _userManager.Users
            .SingleOrDefaultAsync(u => u.PhoneNumber == regRequest.PhoneNumber);
        if (userWithSamePhone != null)
        {
            return (false, $"Phone is already registered.");
        }

        var user = new User
        {
            UserName = "temp",
            FirstName = regRequest.FirstName,
            LastName = regRequest.LastName,
            Patronymic = regRequest.Patronymic,
            Email = regRequest.Email,
            PhoneNumber = regRequest.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, regRequest.Password);
        if (!result.Succeeded)
        {
            return (false, $"{result.Errors?.First().Description}");
        }
        
        await _userManager.AddToRoleAsync(user, Constants.Identity.DefaultRole.ToString());
        return (true, $"User registered with email {user.Email}.");
    }

    public async Task<(bool succeeded, AuthenticationResponse authResponse,
        string? refreshToken)> AuthenticateAsync(AuthenticationRequest authRequest)
    {
        var authResponse = new AuthenticationResponse();

        User user;

        user = await _userManager.FindByEmailAsync(authRequest.Email);

        if (user == null)
        {
            authResponse.Message = $"No accounts registered with {authRequest.Email}.";
            return (false, authResponse, null);
        }

        if (!await _userManager.CheckPasswordAsync(user, authRequest.Password))
        {
            authResponse.Message = $"Incorrect email or password.";
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

    public async Task<(bool succeeded, AuthenticationResponse authResponse,
            string? refreshToken)> RenewRefreshTokenAsync(string? token)
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
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Name, user.LastName + user.FirstName + user.Patronymic),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
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
