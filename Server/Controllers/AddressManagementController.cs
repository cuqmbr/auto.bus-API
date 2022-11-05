using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Controllers;

[Route("api/addresses")]
[ApiController]
public class AddressManagementController : ControllerBase
{
    private readonly IAddressManagementService _addressManagementService;
    
    public AddressManagementController(IAddressManagementService addressManagementService)
    {
        _addressManagementService = addressManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddAddress(CreateAddressDto address)
    {
        var result = await _addressManagementService.AddAddress(address);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return CreatedAtAction(nameof(GetAddress), new {id = result.address.Id}, result.address);
    }

    [HttpGet]
    public async Task<IActionResult> GetAddresses([FromQuery] AddressParameters parameters)
    {
        var result = await _addressManagementService.GetAddresses(parameters);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.addresses);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAddress(int id, [FromQuery] string? fields)
    {
        if (!await _addressManagementService.IsAddressExists(id))
        {
            return NotFound();
        }

        var result = await _addressManagementService.GetAddress(id, fields);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }

        return Ok(result.address);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoute(int id, UpdateAddressDto address)
    {
        if (id != address.Id)
        {
            return BadRequest();
        }
        
        var result = await _addressManagementService.UpdateAddress(address);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return Ok(result.address);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoute(int id)
    {
        if (!await _addressManagementService.IsAddressExists(id))
        {
            return NotFound();
        }
        
        var result = await _addressManagementService.DeleteAddress(id);
        
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return NoContent();
    }
}