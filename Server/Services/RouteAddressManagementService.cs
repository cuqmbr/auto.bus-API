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

public class RouteAddressManagementService : IRouteAddressManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _routeAddressSortHelper;
    private readonly IDataShaper<RouteAddressDto> _routeAddressDataShaper;
    private readonly IPager<ExpandoObject> _pager;

    public RouteAddressManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<ExpandoObject> routeAddressSortHelper, 
        IDataShaper<RouteAddressDto> routeAddressDataShaper, IPager<ExpandoObject> pager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _routeAddressSortHelper = routeAddressSortHelper;
        _routeAddressDataShaper = routeAddressDataShaper;
        _pager = pager;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, RouteAddressDto routeAddress)> AddRouteAddress(CreateRouteAddressDto createRouteAddressDto)
    {
        var routeAddress = _mapper.Map<RouteAddress>(createRouteAddressDto);
    
        await _dbContext.RouteAddresses.AddAsync(routeAddress);
        await _dbContext.SaveChangesAsync();
    
        return (true, null, _mapper.Map<RouteAddressDto>(routeAddress));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> routeAddresses,
            PagingMetadata<ExpandoObject> pagingMetadata)> GetRouteAddresses(RouteAddressParameters parameters)
    {
        var dbRouteAddresses = _dbContext.RouteAddresses
            .Include(ra => ra.Route)
            .Include(ra => ra.Address)
            .ThenInclude(a => a.City).ThenInclude(c => c.State)
            .ThenInclude(s => s.Country)
            .AsQueryable();

        FilterByRouteAddressRouteId(ref dbRouteAddresses, parameters.RouteId);
        FilterByRouteAddressAddressId(ref dbRouteAddresses, parameters.AddressId);

        var routeAddressDtos = _mapper.ProjectTo<RouteAddressDto>(dbRouteAddresses);
        var shapedData = _routeAddressDataShaper.ShapeData(routeAddressDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _routeAddressSortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null!, null!);
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);
        
        return (true, null, shapedData, pagingMetadata);
        
        void FilterByRouteAddressRouteId(ref IQueryable<RouteAddress> routeAddresses,
            int? routeId)
        {
            if (!routeAddresses.Any() || routeId == null)
            {
                return;
            }

            routeAddresses = routeAddresses.Where(ra => ra.RouteId == routeId);
        }
        
        void FilterByRouteAddressAddressId(ref IQueryable<RouteAddress> routeAddresses,
            int? addressId)
        {
            if (!routeAddresses.Any() || addressId == null)
            {
                return;
            }

            routeAddresses = routeAddresses.Where(ra => ra.AddressId == addressId);
        }
    }
    
    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject routeAddress)> GetRouteAddress(int id, string? fields)
    {
        if (!await IsRouteAddressExists(id))
        {
            return (false, new NotFoundResult(), null!);
        }
        
        var dbRouteAddress = await _dbContext.RouteAddresses.Where(ra => ra.Id == id)
            .Include(ra => ra.Route)
            .Include(ra => ra.Address)
            .ThenInclude(a => a.City).ThenInclude(c => c.State)
            .ThenInclude(s => s.Country)
            .FirstAsync();

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = RouteAddressParameters.DefaultFields;
        }
        
        var routeAddressDto = _mapper.Map<RouteAddressDto>(dbRouteAddress);
        var shapedData = _routeAddressDataShaper.ShapeData(routeAddressDto, fields);

        return (true, null, shapedData);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, RouteAddressDto routeAddress)> UpdateRouteAddress(UpdateRouteAddressDto updateRouteAddressDto)
    {
        var routeAddress = _mapper.Map<RouteAddress>(updateRouteAddressDto);
        _dbContext.Entry(routeAddress).State = EntityState.Modified;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsRouteAddressExists(updateRouteAddressDto.Id))
            {
                return (false, new NotFoundResult(), null!);
            }
            
            throw;
        }

        var dbRouteAddress = await _dbContext.RouteAddresses.FirstAsync(ra => ra.Id == routeAddress.Id);
        
        return (true, null, _mapper.Map<RouteAddressDto>(dbRouteAddress));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteRouteAddress(int id)
    {
        var dbRouteAddress = await _dbContext.RouteAddresses.FirstOrDefaultAsync(ra => ra.Id == id);
    
        if (dbRouteAddress == null)
        {
            return (false, new NotFoundResult());
        }
    
        _dbContext.RouteAddresses.Remove(dbRouteAddress);
        await _dbContext.SaveChangesAsync();
    
        return (true, null);
    }

    public async Task<bool> IsRouteAddressExists(int id)
    {
        return await _dbContext.RouteAddresses.AnyAsync(ra => ra.Id == id);
    }
}