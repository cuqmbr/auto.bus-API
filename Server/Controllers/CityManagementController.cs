using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Controllers;

[Route("api/cities")]
[ApiController]
public class CityManagementController : ControllerBase
{
    private readonly ICityManagementService _cityManagementService;
    
    public CityManagementController(ICityManagementService cityManagementService)
    {
        _cityManagementService = cityManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddCity(CreateCityDto city)
    {
        var result = await _cityManagementService.AddCity(city);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return CreatedAtAction(nameof(GetCity), new {id = result.city.Id}, result.city);
    }

    [HttpGet]
    public async Task<IActionResult> GetCities([FromQuery] CityParameters parameters)
    {
        var result = await _cityManagementService.GetCities(parameters);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.cities);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCity(int id, [FromQuery] string? fields)
    {
        if (!await _cityManagementService.IsCityExists(id))
        {
            return NotFound();
        }

        var result = await _cityManagementService.GetCity(id, fields);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }

        return Ok(result.city);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCountry(int id, UpdateCityDto city)
    {
        if (id != city.Id)
        {
            return BadRequest();
        }
        
        var result = await _cityManagementService.UpdateCity(city);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return Ok(result.city);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        if (!await _cityManagementService.IsCityExists(id))
        {
            return NotFound();
        }
        
        var result = await _cityManagementService.DeleteCity(id);
        
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return NoContent();
    }
}