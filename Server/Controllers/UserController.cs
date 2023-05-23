using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters.Objects;

namespace Server.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UserController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }
    
    [HttpPost]
    public async Task<IActionResult> AddUser(CreateUserDto User)
    {
        var result = await _userManagementService.AddUser(User);
    
        if (!result.isSucceeded)
        {
            return result.actionResult;
        }
    
        return CreatedAtAction(nameof(GetUser), new {id = result.user.Id}, result.user);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] UserParameters parameters)
    {
        var result = await _userManagementService.GetUsers(parameters);

        if (!result.isSucceeded)
        {
            return result.actionResult;
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.users);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id, [FromQuery] string? fields)
    {
        var result = await _userManagementService.GetUser(id, fields);

        if (!result.isSucceeded)
        {
            return result.actionResult;
        }

        return Ok(result.user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, UpdateUserDto user)
    {
        var result = await _userManagementService.UpdateUser(id, user);
    
        if (!result.isSucceeded)
        {
            return result.actionResult;
        }
    
        return Ok(result.user);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await _userManagementService.DeleteUser(id);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return NoContent();
    }
}