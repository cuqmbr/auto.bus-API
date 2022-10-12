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

    public async
        Task<(bool isSucceed, string message, IEnumerable<CountryDto> countries,
            PagingMetadata<Country> pagingMetadata)> GetCountries(CountryParameters parameters)
    {
        var dbCountries = await _dbContext.Countries
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var pagingMetadata = new PagingMetadata<Country>(_dbContext.Countries,
            parameters.PageNumber, parameters.PageSize);

        return (true, "", dbCountries.ConvertAll(c => _mapper.Map<CountryDto>(c)),
            pagingMetadata);
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