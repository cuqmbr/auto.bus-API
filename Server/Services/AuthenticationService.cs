using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Server.Constants;
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
    
    public AuthenticationService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<Jwt> jwt)
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

    public async Task<AuthenticationResponse> GetTokenAsync(AuthenticationRequest authRequest)
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

        return authResponse;
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
}