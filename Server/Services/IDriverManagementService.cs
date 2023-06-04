using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;
using SharedModels.Requests;

namespace Server.Services;

public interface IDriverManagementService
{
    
    Task<(bool isSucceeded, IActionResult actionResult)> RegisterDriver(DriverRegistrationRequest request);
    
    Task<(bool isSucceeded, IActionResult actionResult, IEnumerable<ExpandoObject> drivers, PagingMetadata<ExpandoObject> pagingMetadata)> 
        GetDrivers(CompanyDriverParameters parameters);
    
    Task<(bool isSucceeded, IActionResult actionResult, ExpandoObject driver)>
        GetDriver(string driverId, string? fields);

    Task<(bool isSucceed, IActionResult actionResult)> DeleteDriver(string driverId);
}