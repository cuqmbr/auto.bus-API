using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;
using Route = Server.Models.Route;

namespace Server.Services;

public interface IRouteManagementService
{
    Task<(bool isSucceed, string message, RouteDto route)> AddRoute(CreateRouteDto createRouteDto);
    Task<(bool isSucceed, IActionResult? actionResult, RouteWithAddressesDto route)> AddRouteWithAddresses(CreateRouteWithAddressesDto createRouteWithAddressesDto);
    Task<(bool isSucceed, string message, IEnumerable<ExpandoObject> routes,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetRoutes(RouteParameters parameters); 
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> routes,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetRoutesWithAddresses(RouteWithAddressesParameters parameters); 
    Task<(bool isSucceed, string message, ExpandoObject route)> GetRoute(int id, string? fields);
    Task<(bool isSucceed, string message, ExpandoObject route)> GetRouteWithAddresses(int id, string? fields);
    Task<(bool isSucceed, string message, UpdateRouteDto route)> UpdateRoute(UpdateRouteDto updateRouteDto);
    Task<(bool isSucceed, string message)> DeleteRoute(int id);
    Task<bool> IsRouteExists(int id);
}