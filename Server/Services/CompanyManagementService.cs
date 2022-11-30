using System.Dynamic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public class CompanyManagementService : ICompanyManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _companySortHelper;
    private readonly IDataShaper<CompanyDto> _companyDataShaper;
    private readonly IPager<ExpandoObject> _pager;

    public CompanyManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<ExpandoObject> companySortHelper, 
        IDataShaper<CompanyDto> companyDataShaper, IPager<ExpandoObject> pager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _companySortHelper = companySortHelper;
        _companyDataShaper = companyDataShaper;
        _pager = pager;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, CompanyDto company)> AddCompany(CreateCompanyDto createCompanyDto)
    {
        var company = _mapper.Map<Company>(createCompanyDto);
    
        await _dbContext.Companies.AddAsync(company);
        await _dbContext.SaveChangesAsync();
    
        return (true, null, _mapper.Map<CompanyDto>(company));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> companies,
            PagingMetadata<ExpandoObject> pagingMetadata)> GetCompanies(CompanyParameters parameters)
    {
        var dbCompanies = _dbContext.Companies
            .AsQueryable();

        SearchByAllCompanyFields(ref dbCompanies, parameters.Search);
        FilterByCompanyName(ref dbCompanies, parameters.Name);
        FilterByCompanyOwnerId(ref dbCompanies, parameters.OwnerId);

        var companyDtos = _mapper.ProjectTo<CompanyDto>(dbCompanies);
        var shapedData = _companyDataShaper.ShapeData(companyDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _companySortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null!, null!);
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);
        
        return (true, null, shapedData, pagingMetadata);

        void SearchByAllCompanyFields(ref IQueryable<Company> companies,
            string? search)
        {
            if (!companies.Any() || String.IsNullOrWhiteSpace(search))
            {
                return;
            }

            companies = companies.Where(c =>
                c.Name.ToLower().Contains(search.ToLower()));
        }
        
        void FilterByCompanyName(ref IQueryable<Company> companies,
            string? name)
        {
            if (!companies.Any() || String.IsNullOrWhiteSpace(name))
            {
                return;
            }

            companies = companies.Where(c => c.Name == name);
        }
        
        void FilterByCompanyOwnerId(ref IQueryable<Company> companies,
            string? ownerId)
        {
            if (!companies.Any() || String.IsNullOrWhiteSpace(ownerId))
            {
                return;
            }

            companies = companies.Where(c => c.OwnerId == ownerId);
        }
    }
    
    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject company)> GetCompany(int id, string? fields)
    {
        if (!await IsCompanyExists(id))
        {
            return (false, new NotFoundResult(), null!);
        }
        
        var dbCompany = await _dbContext.Companies.Where(c => c.Id == id)
            .FirstAsync();

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = CompanyParameters.DefaultFields;
        }
        
        var companyDto = _mapper.Map<CompanyDto>(dbCompany);
        var shapedData = _companyDataShaper.ShapeData(companyDto, fields);

        return (true, null, shapedData);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, CompanyDto company)> UpdateCompany(UpdateCompanyDto updateCompanyDto)
    {
        var company = _mapper.Map<Company>(updateCompanyDto);
        _dbContext.Entry(company).State = EntityState.Modified;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsCompanyExists(updateCompanyDto.Id))
            {
                return (false, new BadRequestResult(), null!);
            }
        }

        var dbCompany = await _dbContext.Companies.FirstAsync(c => c.Id == company.Id);
        
        return (true, null, _mapper.Map<CompanyDto>(dbCompany));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteCompany(int id)
    {
        var dbCompany = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == id);
    
        if (dbCompany == null)
        {
            return (false, new NotFoundResult());
        }
    
        _dbContext.Companies.Remove(dbCompany);
        await _dbContext.SaveChangesAsync();
    
        return (true, null);
    }

    public async Task<bool> IsCompanyExists(int id)
    {
        return await _dbContext.Companies.AnyAsync(c => c.Id == id);
    }
}