using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Services;

public interface ICountryManagementService
{
    Task<(bool isSucceed, string message, CountryDto country)> AddCountry(CreateCountryDto createCountryDto);
    Task<(bool isSucceed, string message, IEnumerable<CountryDto> countries,
        PagingMetadata<Country> pagingMetadata)> GetCountries(CountryParameters parameters);
    Task<(bool isSucceed, string message, CountryDto country)> GetCountry(int id, string? fields);
    Task<(bool isSucceed, string message, UpdateCountryDto country)> UpdateCountry(UpdateCountryDto updateCountryDto);
    Task<(bool isSucceed, string message)> DeleteCountry(int id);
    Task<bool> IsCountryExists(int id);
}