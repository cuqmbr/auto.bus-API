using AutoMapper;
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
    private readonly ISortHelper<Company> _companySortHelper;
    private readonly IDataShaper<Company> _companyDataShaper;

    public CompanyManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<Company> companySortHelper, 
        IDataShaper<Company> companyDataShaper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _companySortHelper = companySortHelper;
        _companyDataShaper = companyDataShaper;
    }

    public async Task<(bool isSucceed, string message, CompanyDto company)> AddCompany(CreateCompanyDto createCompanyDto)
    {
        var company = _mapper.Map<Company>(createCompanyDto);
    
        await _dbContext.Companies.AddAsync(company);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty, _mapper.Map<CompanyDto>(company));
    }

    public async Task<(bool isSucceed, string message, IEnumerable<CompanyDto> companies,
            PagingMetadata<Company> pagingMetadata)> GetCompanies(CompanyParameters parameters)
    {
        var dbCompanies = _dbContext.Companies
            .AsQueryable();

        var v = dbCompanies.Any();

        SearchByAllCompanyFields(ref dbCompanies, parameters.Search);
        FilterByCompanyName(ref dbCompanies, parameters.Name);
        FilterByCompanyOwnerId(ref dbCompanies, parameters.OwnerId);
        
        try
        {
            dbCompanies = _companySortHelper.ApplySort(dbCompanies, parameters.Sort);
            
            // By calling Any() we will check if LINQ to Entities Query will be
            // executed. If not it will throw an InvalidOperationException exception
            var isExecuted = dbCompanies.Any();
        }
        catch (Exception e)
        {
            return (false, "Invalid sorting string", null, null)!;
        }

        var pagingMetadata = ApplyPaging(ref dbCompanies, parameters.PageNumber,
            parameters.PageSize);

        var shapedCompaniesData = _companyDataShaper.ShapeData(dbCompanies, parameters.Fields);
        var companyDtos = shapedCompaniesData.ToList().ConvertAll(c => _mapper.Map<CompanyDto>(c));
        
        return (true, "", companyDtos, pagingMetadata);

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

        PagingMetadata<Company> ApplyPaging(ref IQueryable<Company> companies,
            int pageNumber, int pageSize)
        {
            var metadata = new PagingMetadata<Company>(companies,
                pageNumber, pageSize);
            
            companies = companies
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return metadata;
        }
    }
    
    public async Task<(bool isSucceed, string message, CompanyDto company)> GetCompany(int id, string? fields)
    {
        var dbCompany = await _dbContext.Companies.Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (dbCompany == null)
        {
            return (false, $"Company doesn't exist", null)!;
        }
        
        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = CompanyParameters.DefaultFields;
        }
        
        var shapedCompanyData = _companyDataShaper.ShapeData(dbCompany, fields);
        var companyDto = _mapper.Map<CompanyDto>(shapedCompanyData);

        return (true, "", companyDto);
    }

    public async Task<(bool isSucceed, string message, UpdateCompanyDto company)> UpdateCompany(UpdateCompanyDto updateCompanyDto)
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
                return (false, $"Company with id:{updateCompanyDto.Id} doesn'c exist", null)!;
            }
            
            throw;
        }

        var dbCompany = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == company.Id);
        
        return (true, String.Empty, _mapper.Map<UpdateCompanyDto>(dbCompany));
    }

    public async Task<(bool isSucceed, string message)> DeleteCompany(int id)
    {
        var dbCompany = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == id);
    
        if (dbCompany == null)
        {
            return (false, $"Company with id:{id} doesn't exist");
        }
    
        _dbContext.Companies.Remove(dbCompany);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty);
    }

    public async Task<bool> IsCompanyExists(int id)
    {
        return await _dbContext.Companies.AnyAsync(c => c.Id == id);
    }
}