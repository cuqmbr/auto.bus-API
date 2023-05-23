using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface ICountryManagementService
{
    Task<(bool isSucceed, IActionResult? actionResult, CountryDto country)> AddCountry(CreateCountryDto createCountryDto);
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> countries,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetCountries(CountryParameters parameters);
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject country)> GetCountry(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, CountryDto country)> UpdateCountry(UpdateCountryDto updateCountryDto);
    Task<(bool isSucceed, IActionResult? actionResult)> DeleteCountry(int id);
    Task<bool> IsCountryExists(int id);
}
