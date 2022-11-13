using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Services;

public class CityManagementService : ICityManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<City> _citySortHelper;
    private readonly IDataShaper<City> _cityDataShaper;

    public CityManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<City> citySortHelper, 
        IDataShaper<City> cityDataShaper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _citySortHelper = citySortHelper;
        _cityDataShaper = cityDataShaper;
    }

    public async Task<(bool isSucceed, string message, CityDto city)> AddCity(CreateCityDto createCityDto)
    {
        var city = _mapper.Map<City>(createCityDto);
    
        await _dbContext.Cities.AddAsync(city);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty, _mapper.Map<CityDto>(city));
    }

    public async Task<(bool isSucceed, string message, IEnumerable<CityDto> cities,
            PagingMetadata<City> pagingMetadata)> GetCities(CityParameters parameters)
    {
        var dbCities = _dbContext.Cities.Include(c => c.State)
            .ThenInclude(s => s.Country).Include(c => c.Addresses)
            .AsQueryable();
        
        SearchByAllCityFields(ref dbCities, parameters.Search);
        FilterByCityName(ref dbCities, parameters.Name);
        FilterByStateId(ref dbCities, parameters.StateId);

        try
        {
            dbCities = _citySortHelper.ApplySort(dbCities, parameters.Sort);
            
            // By calling Any() we will check if LINQ to Entities Query will be
            // executed. If not it will throw an InvalidOperationException exception
            var isExecuted = dbCities.Any();
        }
        catch (Exception e)
        {
            return (false, "Invalid sorting string", null, null)!;
        }

        var pagingMetadata = ApplyPaging(ref dbCities, parameters.PageNumber,
            parameters.PageSize);

        var shapedCitiesData = _cityDataShaper.ShapeData(dbCities, parameters.Fields);
        var cityDtos = shapedCitiesData.ToList().ConvertAll(s => _mapper.Map<CityDto>(s));
        
        return (true, "", cityDtos, pagingMetadata);

        void SearchByAllCityFields(ref IQueryable<City> cities,
            string? search)
        {
            if (!cities.Any() || String.IsNullOrWhiteSpace(search))
            {
                return;
            }

            cities = cities.Where(s =>
                s.Name.ToLower().Contains(search.ToLower()));
        }
        
        void FilterByStateId(ref IQueryable<City> cities,
            int? stateId)
        {
            if (!cities.Any() || stateId == null)
            {
                return;
            }

            cities = cities.Where(s => s.StateId == stateId);
        }
        
        void FilterByCityName(ref IQueryable<City> cities,
            string? cityName)
        {
            if (!cities.Any() || String.IsNullOrWhiteSpace(cityName))
            {
                return;
            }

            cities = cities.Where(s =>
                s.Name.ToLower().Contains(cityName.Trim().ToLower()));
        }

        PagingMetadata<City> ApplyPaging(ref IQueryable<City> cities,
            int pageNumber, int pageSize)
        {
            var metadata = new PagingMetadata<City>(cities,
                pageNumber, pageSize);
            
            cities = cities
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return metadata;
        }
    }
    
    public async Task<(bool isSucceed, string message, CityDto city)> GetCity(int id, string? fields)
    {
        var dbCity = await _dbContext.Cities.Where(s => s.Id == id)
            .Include(c => c.State).ThenInclude(s => s.Country)
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync();

        if (dbCity == null)
        {
            return (false, $"City doesn't exist", null)!;
        }
        
        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = CityParameters.DefaultFields;
        }
        
        var shapedCityData = _cityDataShaper.ShapeData(dbCity, fields);
        var cityDto = _mapper.Map<CityDto>(shapedCityData);

        return (true, "", cityDto);
    }

    public async Task<(bool isSucceed, string message, UpdateCityDto city)> UpdateCity(UpdateCityDto updateCityDto)
    {
        var city = _mapper.Map<City>(updateCityDto);
        _dbContext.Entry(city).State = EntityState.Modified;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsCityExists(updateCityDto.Id))
            {
                return (false, $"City with id:{updateCityDto.Id} doesn't exist", null)!;
            }
            
            throw;
        }

        var dbCity = await _dbContext.Cities.FirstOrDefaultAsync(s => s.Id == city.Id);
        
        return (true, String.Empty, _mapper.Map<UpdateCityDto>(dbCity));
    }

    public async Task<(bool isSucceed, string message)> DeleteCity(int id)
    {
        var dbCity = await _dbContext.Cities.FirstOrDefaultAsync(s => s.Id == id);
    
        if (dbCity == null)
        {
            return (false, $"City with id:{id} doesn't exist");
        }
    
        _dbContext.Cities.Remove(dbCity);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty);
    }

    public async Task<bool> IsCityExists(int id)
    {
        return await _dbContext.Cities.AnyAsync(s => s.Id == id);
    }
}