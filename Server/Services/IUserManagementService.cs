using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IUserManagementService
{
    Task<(bool isSucceeded, IActionResult actionResult, UserDto user)> AddUser(CreateUserDto createUserDto);
    
    Task<(bool isSucceeded, IActionResult actionResult, IEnumerable<ExpandoObject> users, PagingMetadata<ExpandoObject> pagingMetadata)> 
        GetUsers(UserParameters parameters);

    Task<(bool isSucceeded, IActionResult actionResult, ExpandoObject user)> GetUser(string id, string? fields);

    Task<(bool isSucceeded, IActionResult actionResult, UserDto user)>
        UpdateUser(string id, UpdateUserDto updateUserDto);
    
    Task<(bool isSucceed, IActionResult actionResult)> DeleteUser(string id);
}