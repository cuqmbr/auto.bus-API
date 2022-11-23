using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public interface ICompanyManagementService
{
    Task<(bool isSucceed, string message, CompanyDto company)> AddCompany(CreateCompanyDto createCompanyDto);
    Task<(bool isSucceed, string message, IEnumerable<CompanyDto> companies,
        PagingMetadata<Company> pagingMetadata)> GetCompanies(CompanyParameters parameters); 
    Task<(bool isSucceed, string message, CompanyDto company)> GetCompany(int id, string? fields);
    Task<(bool isSucceed, string message, UpdateCompanyDto company)> UpdateCompany(UpdateCompanyDto updateCompanyDto);
    Task<(bool isSucceed, string message)> DeleteCompany(int id);
    Task<bool> IsCompanyExists(int id);
}