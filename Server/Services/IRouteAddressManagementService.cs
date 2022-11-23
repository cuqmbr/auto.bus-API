using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IRouteAddressManagementService
{
    Task<(bool isSucceed, string message, RouteAddressDto routeAddress)> AddRouteAddress(CreateRouteAddressDto createRouteAddressDto);
    Task<(bool isSucceed, string message, IEnumerable<RouteAddressDto> routeAddresses,
        PagingMetadata<RouteAddress> pagingMetadata)> GetRouteAddresses(RouteAddressParameters parameters); 
    Task<(bool isSucceed, string message, RouteAddressDto routeAddress)> GetRouteAddress(int id, string? fields);
    Task<(bool isSucceed, string message, UpdateRouteAddressDto routeAddress)> UpdateRouteAddress(UpdateRouteAddressDto updateRouteAddressDto);
    Task<(bool isSucceed, string message)> DeleteRouteAddress(int id);
    Task<bool> IsRouteAddressExists(int id);
}