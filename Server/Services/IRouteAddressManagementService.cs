using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IRouteAddressManagementService
{
    Task<(bool isSucceed, IActionResult? actionResult, RouteAddressDto routeAddress)> AddRouteAddress(CreateRouteAddressDto createRouteAddressDto);
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> routeAddresses,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetRouteAddresses(RouteAddressParameters parameters); 
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject routeAddress)> GetRouteAddress(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, RouteAddressDto routeAddress)> UpdateRouteAddress(UpdateRouteAddressDto updateRouteAddressDto);
    Task<(bool isSucceed, IActionResult? actionResult)> DeleteRouteAddress(int id);
    Task<bool> IsRouteAddressExists(int id);
}