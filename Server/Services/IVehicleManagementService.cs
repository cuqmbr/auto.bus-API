using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface IVehicleManagementService
{
    Task<(bool isSucceed, IActionResult? actionResult, VehicleDto vehicle)> AddVehicle(CreateVehicleDto createVehicleDto);
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> vehicles,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetVehicles(VehicleParameters parameters); 
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject vehicle)> GetVehicle(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, VehicleDto vehicle)> UpdateVehicle(UpdateVehicleDto updateVehicleDto);
    Task<(bool isSucceed, IActionResult? actionResult)> DeleteVehicle(int id);
    Task<bool> IsVehicleExists(int id);
}