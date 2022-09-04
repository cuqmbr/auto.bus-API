using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Server.Constants;
using Server.Entities;
using Server.Models;
using Server.Settings;
using SharedModels.Requests;
using SharedModels.Responses;

namespace Server.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly Jwt _jwt;
    
    public AuthenticationService(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager, IOptions<Jwt> jwt)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwt = jwt.Value;
    }

    public async Task<(bool, string)> RegisterAsync(RegistrationRequest regRequest)
    {
        var userWithSameEmail = await _userManager.FindByEmailAsync(regRequest.Email);
        if (userWithSameEmail != null)
        {
            return (false, $"Email {regRequest.Email} is already registered.");
        }

        var user = new ApplicationUser {UserName = regRequest.Username, Email = regRequest.Email};

        var result = await _userManager.CreateAsync(user, regRequest.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, Authorization.DefaultRole.ToString());
            return (true, $"User registered with email {user.Email}.");
        }
        else
        {
            return (false, $"{result.Errors?.First().Description}.");
        }
    }

    public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest authRequest)
    {
        var authResponse = new AuthenticationResponse();
        
        var user = await _userManager.FindByEmailAsync(authRequest.Email);

        if (user == null)
        {
            authResponse.IsAuthenticated = false;
            authResponse.Message = $"No accounts registered with {authRequest.Email}.";
            return authResponse;
        }

        if (!await _userManager.CheckPasswordAsync(user, authRequest.Password))
        {
            authResponse.IsAuthenticated = false;
            authResponse.Message = $"Incorrect login or password.";
            return authResponse;
        }

        authResponse.IsAuthenticated = true;
        authResponse.Email = user.Email;
        authResponse.UserName = user.UserName;
        var roles = await _userManager.GetRolesAsync(user);
        authResponse.Roles = roles.ToList();
        var jwtSecurityToken = await CreateJwtToken(user);
        authResponse.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        if (user.RefreshTokens.Any(t => t.IsActive))
        {
            var activeRefreshToken =
                user.RefreshTokens.First(t => t.IsActive);
            authResponse.RefreshToken = activeRefreshToken.Token;
            authResponse.RefreshTokenExpiration = activeRefreshToken.Expires;
        }
        else
        {
            var refreshToken = CreateRefreshToken();
            authResponse.RefreshToken = refreshToken.Token;
            authResponse.RefreshTokenExpiration = refreshToken.Expires;
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);
        }
        
        return authResponse;
    }

    public async Task<AuthenticationResponse> RenewRefreshTokenAsync(string? token)
    {
        var authResponse = new AuthenticationResponse();

        var user = await _userManager.Users.SingleOrDefaultAsync(u =>
            u.RefreshTokens.Any(t => t.Token == token));

        if (user == null)
        {
            authResponse.IsAuthenticated = false;
            authResponse.Message = "Refresh token did not mach any user.";
            return authResponse;
        }

        var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

        if (!refreshToken.IsActive)
        {
            authResponse.IsAuthenticated = false;
            authResponse.Message = "Refresh token expired.";
            return authResponse;
        }
        
        //Revoke Current Refresh Token
        refreshToken.Revoked = DateTime.UtcNow;
        
        //Generate new Refresh Token and save to Database
        var newRefreshToken = CreateRefreshToken();
        user.RefreshTokens.Add(newRefreshToken);
        await _userManager.UpdateAsync(user);
        
        //Generates new jwt
        authResponse.IsAuthenticated = true;
        authResponse.Email = user.Email;
        authResponse.UserName = user.UserName;
        
        var roles = await _userManager.GetRolesAsync(user);
        authResponse.Roles = roles.ToList();
        
        var jwtSecurityToken = await CreateJwtToken(user);
        authResponse.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        
        authResponse.RefreshToken = newRefreshToken.Token;
        authResponse.RefreshTokenExpiration = newRefreshToken.Expires;
        
        return authResponse;
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
    
    private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
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
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
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
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(_jwt.RefreshTokenValidityInDays)
        };
    }
}