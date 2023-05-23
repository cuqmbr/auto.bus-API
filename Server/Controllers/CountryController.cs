using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters.Objects;

namespace Server.Controllers;

[Authorize]
[Route("api/countries")]
[ApiController]
public class CountryController : ControllerBase
{
    private readonly ICountryManagementService _countryManagementService;
    
    public CountryController(ICountryManagementService countryManagementService)
    {
        _countryManagementService = countryManagementService;
    }

    [Authorize(Policy = "AdministratorAccess")]
    [HttpPost]
    public async Task<IActionResult> AddCountry(CreateCountryDto country)
    {
        var result = await _countryManagementService.AddCountry(country);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return CreatedAtAction(nameof(GetCountry), new {id = result.country.Id}, result.country);
    }

    [Authorize(Policy = "CompanyAccess")]
    [HttpGet]
    public async Task<IActionResult> GetCountries([FromQuery] CountryParameters parameters)
    {
        var result = await _countryManagementService.GetCountries(parameters);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.countries);
    }
    
    [Authorize(Policy = "CompanyAccess")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCountry(int id, [FromQuery] string? fields)
    {
        var result = await _countryManagementService.GetCountry(id, fields);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.country);
    }

    [Authorize(Policy = "AdministratorAccess")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCountry(int id, UpdateCountryDto country)
    {
        var result = await _countryManagementService.UpdateCountry(country);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return Ok(result.country);
    }
    
    [Authorize(Policy = "AdministratorAccess")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var result = await _countryManagementService.DeleteCountry(id);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return NoContent();
    }
}