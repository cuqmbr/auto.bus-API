using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface ICompanyManagementService
{
    Task<(bool isSucceed, IActionResult? actionResult, CompanyDto company)> AddCompany(CreateCompanyDto createCompanyDto);
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> companies,
        PagingMetadata<ExpandoObject> pagingMetadata)> GetCompanies(CompanyParameters parameters); 
    Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject company)> GetCompany(int id, string? fields);
    Task<(bool isSucceed, IActionResult? actionResult, CompanyDto company)> UpdateCompany(UpdateCompanyDto updateCompanyDto);
    Task<(bool isSucceed, IActionResult? actionResult)> DeleteCompany(int id);
    Task<bool> IsCompanyExists(int id);
}