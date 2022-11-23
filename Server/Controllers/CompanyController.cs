using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters.Objects;

namespace Server.Controllers;

[Route("api/companies")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ICompanyManagementService _companyManagementService;
    
    public CompanyController(ICompanyManagementService companyManagementService)
    {
        _companyManagementService = companyManagementService;
    }

    [HttpPost]
    public async Task<IActionResult> AddCompany(CreateCompanyDto company)
    {
        var result = await _companyManagementService.AddCompany(company);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return CreatedAtAction(nameof(GetCompany), new {id = result.company.Id}, result.company);
    }

    [HttpGet]
    public async Task<IActionResult> GetCompanies([FromQuery] CompanyParameters parameters)
    {
        var result = await _companyManagementService.GetCompanies(parameters);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
        
        Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.pagingMetadata));
        
        return Ok(result.companies);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany(int id, [FromQuery] string? fields)
    {
        if (!await _companyManagementService.IsCompanyExists(id))
        {
            return NotFound();
        }

        var result = await _companyManagementService.GetCompany(id, fields);

        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }

        return Ok(result.company);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(int id, UpdateCompanyDto company)
    {
        if (id != company.Id)
        {
            return BadRequest();
        }
        
        var result = await _companyManagementService.UpdateCompany(company);
    
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return Ok(result.company);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        if (!await _companyManagementService.IsCompanyExists(id))
        {
            return NotFound();
        }
        
        var result = await _companyManagementService.DeleteCompany(id);
        
        if (!result.isSucceed)
        {
            return BadRequest(result.message);
        }
    
        return NoContent();
    }
}