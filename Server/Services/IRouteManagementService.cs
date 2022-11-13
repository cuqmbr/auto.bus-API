using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;
using Route = Server.Models.Route;

namespace Server.Services;

public interface IRouteManagementService
{
    Task<(bool isSucceed, string message, RouteDto route)> AddRoute(CreateRouteDto createRouteDto);
    Task<(bool isSucceed, string message, IEnumerable<RouteDto> routes,
        PagingMetadata<Route> pagingMetadata)> GetRoutes(RouteParameters parameters); 
    Task<(bool isSucceed, string message, RouteDto route)> GetRoute(int id, string? fields);
    Task<(bool isSucceed, string message, UpdateRouteDto route)> UpdateRoute(UpdateRouteDto updateRouteDto);
    Task<(bool isSucceed, string message)> DeleteRoute(int id);
    Task<bool> IsRouteExists(int id);
}