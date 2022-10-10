using SharedModels.DataTransferObjects;

namespace Server.Services;

public interface ICountryManagementService
{
    Task<(bool isSucceed, string message, CountryDto country)> AddCountry(CreateCountryDto createCountryDto);

    Task<(bool isSucceed, string message, IEnumerable<CountryDto> countries)> GetCountries();
    Task<(bool isSucceed, string message, CountryDto country)> GetCountry(int id);
    Task<(bool isSucceed, string message, CountryDto country)> UpdateCountry(UpdateCountryDto updateCountryDto);
    Task<(bool isSucceed, string message)> DeleteCountry(int id);
    Task<bool> IsCountryExists(int id);
}