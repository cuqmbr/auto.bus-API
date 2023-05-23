using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IDriverManagementService
{
    
    Task<(bool isSucceeded, IActionResult? actionResult, DriverDto driver)> AddDriver(CreateDriverDto createDriverDto);
    
    Task<(bool isSucceeded, IActionResult? actionResult, IEnumerable<ExpandoObject> drivers, PagingMetadata<ExpandoObject> pagingMetadata)> 
        GetDrivers(CompanyDriverParameters parameters);
    
    Task<(bool isSucceeded, IActionResult? actionResult, ExpandoObject driver)> GetDriver(string id, string? fields);
    
    Task<(bool isSucceeded, IActionResult? actionResult, DriverDto driver)> UpdateDriver(string id, UpdateDriverDto updateDriverDto);
    
    Task<(bool isSucceed, IActionResult? actionResult)> DeleteDriver(string id);
}