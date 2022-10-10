using Microsoft.AspNetCore.Mvc;
using Server.Data;
using Server.Services;
using SharedModels.DataTransferObjects;

namespace Server.Controllers;

[Route("api/countries")]
[ApiController]
public class CountryManagementController : ControllerBase
{
    private readonly ICountryManagementService _countryManagementService;
    
    public CountryManagementController(ICountryManagementService countryManagementService)
    {
        _countryManagementService = countryManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddCountry(CreateCountryDto country)
    {
        var result = await _countryManagementService.AddCountry(country);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return CreatedAtAction(nameof(GetCountry), new {id = result.country.Id}, result.country);
    }

    [HttpGet]
    public async Task<IActionResult> GetCountries()
    {
        var result = await _countryManagementService.GetCountries();

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }

        return Ok(result.countries);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCountry(int id)
    {
        if (!await _countryManagementService.IsCountryExists(id))
        {
            return NotFound();
        }

        var result = await _countryManagementService.GetCountry(id);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }

        return Ok(result.country);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoute(int id, UpdateCountryDto country)
    {
        if (id != country.Id)
        {
            return BadRequest();
        }
        
        var result = await _countryManagementService.UpdateCountry(country);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoute(int id)
    {
        if (!await _countryManagementService.IsCountryExists(id))
        {
            return NotFound();
        }
        
        var result = await _countryManagementService.DeleteCountry(id);
        
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return NoContent();
    }
}
