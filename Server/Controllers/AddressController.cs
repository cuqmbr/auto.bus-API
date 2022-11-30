using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters.Objects;

namespace Server.Controllers;

[Route("api/addresses")]
[ApiController]
public class AddressController : ControllerBase
{
    private readonly IAddressManagementService _addressManagementService;
    
    public AddressController(IAddressManagementService addressManagementService)
    {
        _addressManagementService = addressManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddAddress(CreateAddressDto address)
    {
        var result = await _addressManagementService.AddAddress(address);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return CreatedAtAction(nameof(GetAddress), new {id = result.address.Id}, result.address);
    }

    [HttpGet]
    public async Task<IActionResult> GetAddresses([FromQuery] AddressParameters parameters)
    {
        var result = await _addressManagementService.GetAddresses(parameters);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.addresses);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAddress(int id, [FromQuery] string? fields)
    {
        var result = await _addressManagementService.GetAddress(id, fields);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.address);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(int id, UpdateAddressDto address)
    {
        var result = await _addressManagementService.UpdateAddress(address);
    
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return Ok(result.address);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var result = await _addressManagementService.DeleteAddress(id);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }
    
        return NoContent();
    }
}