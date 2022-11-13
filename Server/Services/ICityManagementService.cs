using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Services;

public interface ICityManagementService
{
    Task<(bool isSucceed, string message, CityDto city)> AddCity(CreateCityDto createCityDto);
    Task<(bool isSucceed, string message, IEnumerable<CityDto> cities,
        PagingMetadata<City> pagingMetadata)> GetCities(CityParameters parameters);
    Task<(bool isSucceed, string message, CityDto city)> GetCity(int id, string? fields);
    Task<(bool isSucceed, string message, UpdateCityDto city)> UpdateCity(UpdateCityDto updateCityDto);
    Task<(bool isSucceed, string message)> DeleteCity(int id);
    Task<bool> IsCityExists(int id);
}