using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Services;

public class CountryManagementService : ICountryManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public CountryManagementService(ApplicationDbContext dbContext,
        IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<(bool isSucceed, string message, CountryDto country)> AddCountry(CreateCountryDto createCountryDto)
    {
        var country = _mapper.Map<Country>(createCountryDto);
    
        await _dbContext.Countries.AddAsync(country);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty, _mapper.Map<CountryDto>(country));
    }

    public async Task<(bool isSucceed, string message, IEnumerable<CountryDto> countries,
            PagingMetadata<Country> pagingMetadata)> GetCountries(CountryParameters parameters)
    {
        var dbCountries = _dbContext.Countries.AsQueryable();
        
        SearchByAllCountryFields(ref dbCountries, parameters.Search);
        SearchByCountryCode(ref dbCountries, parameters.CountryCode);
        SearchByCountryName(ref dbCountries, parameters.CountryName);

        var pagingMetadata = ApplyPaging(ref dbCountries, parameters.PageNumber,
            parameters.PageSize);

        var countryDtos = dbCountries.ToList()
            .ConvertAll(c => _mapper.Map<CountryDto>(c));
        
        return (true, "", countryDtos, pagingMetadata);

        void SearchByAllCountryFields(ref IQueryable<Country> countries,
            string? search)
        {
            if (!countries.Any() || String.IsNullOrWhiteSpace(search))
            {
                return;
            }
            
            var s = search.Trim().ToLower();

            countries = countries.Where(c =>
                c.Code.ToLower().Contains(search) ||
                c.Name.ToLower().Contains(search));
        }
        
        void SearchByCountryCode(ref IQueryable<Country> countries,
            string? countryCode)
        {
            if (!countries.Any() || String.IsNullOrWhiteSpace(countryCode))
            {
                return;
            }

            countries = countries.Where(c =>
                c.Code.ToLower().Contains(countryCode.Trim().ToLower()));
        }
        
        void SearchByCountryName(ref IQueryable<Country> countries,
            string? countryName)
        {
            if (!countries.Any() || String.IsNullOrWhiteSpace(countryName))
            {
                return;
            }

            countries = countries.Where(c =>
                c.Name.ToLower().Contains(countryName.Trim().ToLower()));
        }

        PagingMetadata<Country> ApplyPaging(ref IQueryable<Country> countries,
            int pageNumber, int pageSize)
        {
            var metadata = new PagingMetadata<Country>(countries,
                parameters.PageNumber, parameters.PageSize);
            
            countries = countries
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize);

            return metadata;
        }
    }
    
    public async Task<(bool isSucceed, string message, CountryDto country)> GetCountry(int id)
    {
        var dbCountry = await _dbContext.Countries.Where(c => c.Id == id)
            .ProjectTo<CountryDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (dbCountry == null)
        {
            return (false, $"Country doesn't exist", null)!;
        }

        return (true, "", dbCountry);
    }

    public async Task<(bool isSucceed, string message, CountryDto country)> UpdateCountry(UpdateCountryDto updateCountryDto)
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
                return (false, $"Country with id:{updateCountryDto.Id} doesn't exist", null)!;
            }
            
            throw;
        }

        var dbCountry = await _dbContext.Countries.FirstOrDefaultAsync(c => c.Id == country.Id);
        
        return (true, String.Empty, _mapper.Map<CountryDto>(dbCountry));
    }

    public async Task<(bool isSucceed, string message)> DeleteCountry(int id)
    {
        var dbCountry = await _dbContext.Countries.FirstOrDefaultAsync(c => c.Id == id);
    
        if (dbCountry == null)
        {
            return (false, $"Country with id:{id} doesn't exist");
        }
    
        _dbContext.Countries.Remove(dbCountry);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty);
    }

    public async Task<bool> IsCountryExists(int id)
    {
        return await _dbContext.Countries.AnyAsync(c => c.Id == id);
    }
}