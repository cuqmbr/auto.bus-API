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

public class CityManagementService : ICityManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _citySortHelper;
    private readonly IDataShaper<CityDto> _cityDataShaper;
    private readonly IPager<ExpandoObject> _pager;

    public CityManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<ExpandoObject> citySortHelper, 
        IDataShaper<CityDto> cityDataShaper, IPager<ExpandoObject> pager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _citySortHelper = citySortHelper;
        _cityDataShaper = cityDataShaper;
        _pager = pager;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, CityDto city)> AddCity(CreateCityDto createCityDto)
    {
        var city = _mapper.Map<City>(createCityDto);
    
        await _dbContext.Cities.AddAsync(city);
        await _dbContext.SaveChangesAsync();
    
        return (true, null, _mapper.Map<CityDto>(city));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> cities,
            PagingMetadata<ExpandoObject> pagingMetadata)> GetCities(CityParameters parameters)
    {
        var dbCities = _dbContext.Cities.Include(c => c.State)
            .ThenInclude(s => s.Country).Include(c => c.Addresses)
            .AsQueryable();
        
        SearchByAllCityFields(ref dbCities, parameters.Search);
        FilterByCityName(ref dbCities, parameters.Name);
        FilterByStateId(ref dbCities, parameters.StateId);

        var cityDtos = _mapper.ProjectTo<CityDto>(dbCities);
        var shapedData = _cityDataShaper.ShapeData(cityDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _citySortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception e)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null, null)!;
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);
        
        return (true, null, shapedData, pagingMetadata);

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
    }
    
    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject city)> GetCity(int id, string? fields)
    {
        if (!await IsCityExists(id))
        {
            return (false, new NotFoundResult(), null!);
        }
        
        var dbCity = await _dbContext.Cities.Where(s => s.Id == id)
            .Include(c => c.State).ThenInclude(s => s.Country)
            .Include(c => c.Addresses)
            .FirstAsync();

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = CityParameters.DefaultFields;
        }
        
        var cityDto = _mapper.Map<CityDto>(dbCity);
        var shapedData = _cityDataShaper.ShapeData(cityDto, fields);

        return (true, null, shapedData);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, CityDto city)> UpdateCity(UpdateCityDto updateCityDto)
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
                return (false, new NotFoundResult(), null)!;
            }
        }

        var dbCity = await _dbContext.Cities.FirstAsync(s => s.Id == city.Id);
        
        return (true, null, _mapper.Map<CityDto>(dbCity));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteCity(int id)
    {
        var dbCity = await _dbContext.Cities.FirstOrDefaultAsync(s => s.Id == id);

        if (dbCity == null)
        {
            return (false, new NotFoundResult());
        }
        
        _dbContext.Cities.Remove(dbCity);
        await _dbContext.SaveChangesAsync();
    
        return (true, null);
    }

    public async Task<bool> IsCityExists(int id)
    {
        return await _dbContext.Cities.AnyAsync(s => s.Id == id);
    }
}