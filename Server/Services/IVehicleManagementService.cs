using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IVehicleManagementService
{
    Task<(bool isSucceed, string message, VehicleDto vehicle)> AddVehicle(CreateVehicleDto createVehicleDto);
    Task<(bool isSucceed, string message, IEnumerable<VehicleDto> vehicles,
        PagingMetadata<Vehicle> pagingMetadata)> GetVehicles(VehicleParameters parameters); 
    Task<(bool isSucceed, string message, VehicleDto vehicle)> GetVehicle(int id, string? fields);
    Task<(bool isSucceed, string message, UpdateVehicleDto vehicle)> UpdateVehicle(UpdateVehicleDto updateVehicleDto);
    Task<(bool isSucceed, string message)> DeleteVehicle(int id);
    Task<bool> IsVehicleExists(int id);
}