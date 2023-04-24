using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IRouteManagementService
{
    Task<(bool isSucceed, IActionResult? actionResult, RouteDto route)> AddRoute(CreateRouteDto createRouteDto);
    Task<(bool isSucceed, IActionResult? actionResult, RouteWithAddressesDto route)> AddRouteWithAddresses(CreateRouteWithAddressesDto createRouteWithAddressesDto);
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> routes,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetRoutes(RouteParameters parameters); 
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> routes,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetRoutesWithAddresses(RouteWithAddressesParameters parameters); 
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject route)> GetRoute(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject route)> GetRouteWithAddresses(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, UpdateRouteDto route)> UpdateRoute(UpdateRouteDto updateRouteDto);
    Task<(bool isSucceed, IActionResult? actionResult)> DeleteRoute(int id);
    Task<bool> IsRouteExists(int id);
}
