using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface ICityManagementService
{
    Task<(bool isSucceed, IActionResult? actionResult, CityDto city)> AddCity(CreateCityDto createCityDto);
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> cities,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetCities(CityParameters parameters);
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject city)> GetCity(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, CityDto city)> UpdateCity(UpdateCityDto updateCityDto);
    Task<(bool isSucceed, IActionResult? actionResult)> DeleteCity(int id);
    Task<bool> IsCityExists(int id);
}