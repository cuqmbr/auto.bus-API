using System.Dynamic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;

namespace Server.Services;

public class CountryManagementService : ICountryManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _countrySortHelper;
    private readonly IDataShaper<CountryDto> _countryDataShaper;
    private readonly IPager<ExpandoObject> _pager;

    public CountryManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<ExpandoObject> countrySortHelper, 
        IDataShaper<CountryDto> countryDataShaper, IPager<ExpandoObject> pager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _countrySortHelper = countrySortHelper;
        _countryDataShaper = countryDataShaper;
        _pager = pager;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, CountryDto country)> AddCountry(CreateCountryDto createCountryDto)
    {
        var country = _mapper.Map<Country>(createCountryDto);
    
        await _dbContext.Countries.AddAsync(country);
        await _dbContext.SaveChangesAsync();
    
        return (true, null, _mapper.Map<CountryDto>(country));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> countries,
            PagingMetadata<ExpandoObject> pagingMetadata)> GetCountries(CountryParameters parameters)
    {
        var dbCountries = _dbContext.Countries.Include(c => c.States)
            .ThenInclude(s => s.Cities).ThenInclude(c => c.Addresses)
            .AsQueryable();
        
        SearchByAllCountryFields(ref dbCountries, parameters.Search);
        FilterByCountryCode(ref dbCountries, parameters.Code);
        FilterByCountryName(ref dbCountries, parameters.Name);

        var countryDtos = _mapper.ProjectTo<CountryDto>(dbCountries);
        var shapedData = _countryDataShaper.ShapeData(countryDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _countrySortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null, null)!;
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);
        
        return (true, null, shapedData, pagingMetadata);

        void SearchByAllCountryFields(ref IQueryable<Country> countries,
            string? search)
        {
            if (!countries.Any() || String.IsNullOrWhiteSpace(search))
            {
                return;
            }

            countries = countries.Where(c =>
                c.Code.ToLower().Contains(search.ToLower()) ||
                c.Name.ToLower().Contains(search.ToLower()));
        }
        
        void FilterByCountryCode(ref IQueryable<Country> countries,
            string? countryCode)
        {
            if (!countries.Any() || String.IsNullOrWhiteSpace(countryCode))
            {
                return;
            }

            countries = countries.Where(c =>
                c.Code.ToLower().Contains(countryCode.Trim().ToLower()));
        }
        
        void FilterByCountryName(ref IQueryable<Country> countries,
            string? countryName)
        {
            if (!countries.Any() || String.IsNullOrWhiteSpace(countryName))
            {
                return;
            }

            countries = countries.Where(c =>
                c.Name.ToLower().Contains(countryName.Trim().ToLower()));
        }
    }
    
    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject country)> GetCountry(int id, string? fields)
    {
        if (!await IsCountryExists(id))
        {
            return (false, new NotFoundResult(), null!);
        }
        
        var dbCountry = await _dbContext.Countries.Where(c => c.Id == id)
            .Include(c => c.States).ThenInclude(s => s.Cities)
            .ThenInclude(c => c.Addresses)
            .FirstAsync();

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = CountryParameters.DefaultFields;
        }
        
        var countryDto = _mapper.Map<CountryDto>(dbCountry);
        var shapedData = _countryDataShaper.ShapeData(countryDto, fields);

        return (true, null, shapedData);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, CountryDto country)> UpdateCountry(UpdateCountryDto updateCountryDto)
    {
        var country = _mapper.Map<Country>(updateCountryDto);
        _dbContext.Entry(country).State = EntityState.Modified;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsCountryExists(updateCountryDto.Id))
            {
                return (false, new NotFoundResult(), null!);
            }
        }

        var dbCountry = await _dbContext.Countries.FirstAsync(c => c.Id == country.Id);
        
        return (true, null, _mapper.Map<CountryDto>(dbCountry));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteCountry(int id)
    {
        var dbCountry = await _dbContext.Countries.FirstOrDefaultAsync(c => c.Id == id);
    
        if (dbCountry == null)
        {
            return (false, new NotFoundResult());
        }
    
        _dbContext.Countries.Remove(dbCountry);
        await _dbContext.SaveChangesAsync();
    
        return (true, null);
    }

    public async Task<bool> IsCountryExists(int id)
    {
        return await _dbContext.Countries.AnyAsync(c => c.Id == id);
    }
}