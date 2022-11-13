using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryStringParameters;

namespace Server.Services;

public class RouteAddressManagementService : IRouteAddressManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<RouteAddress> _routeAddressSortHelper;
    private readonly IDataShaper<RouteAddress> _routeAddressDataShaper;

    public RouteAddressManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<RouteAddress> routeAddressSortHelper, 
        IDataShaper<RouteAddress> routeAddressDataShaper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _routeAddressSortHelper = routeAddressSortHelper;
        _routeAddressDataShaper = routeAddressDataShaper;
    }

    public async Task<(bool isSucceed, string message, RouteAddressDto routeAddress)> AddRouteAddress(CreateRouteAddressDto createRouteAddressDto)
    {
        var routeAddress = _mapper.Map<RouteAddress>(createRouteAddressDto);
    
        await _dbContext.RouteAddresses.AddAsync(routeAddress);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty, _mapper.Map<RouteAddressDto>(routeAddress));
    }

    public async Task<(bool isSucceed, string message, IEnumerable<RouteAddressDto> routeAddresses,
            PagingMetadata<RouteAddress> pagingMetadata)> GetRouteAddresses(RouteAddressParameters parameters)
    {
        var dbRouteAddresses = _dbContext.RouteAddresses
            .AsQueryable();

        FilterByRouteAddressRouteId(ref dbRouteAddresses, parameters.RouteId);
        FilterByRouteAddressAddressId(ref dbRouteAddresses, parameters.AddressId);

        try
        {
            dbRouteAddresses = _routeAddressSortHelper.ApplySort(dbRouteAddresses, parameters.Sort);
            
            // By calling Any() we will check if LINQ to Entities Query will be
            // executed. If not it will throw an InvalidOperationException exception
            var isExecuted = dbRouteAddresses.Any();
        }
        catch (Exception e)
        {
            return (false, "Invalid sorting string", null, null)!;
        }

        var pagingMetadata = ApplyPaging(ref dbRouteAddresses, parameters.PageNumber,
            parameters.PageSize);

        var shapedRouteAddressesData = _routeAddressDataShaper.ShapeData(dbRouteAddresses, parameters.Fields);
        var routeAddressDtos = shapedRouteAddressesData.ToList().ConvertAll(ra => _mapper.Map<RouteAddressDto>(ra));
        
        return (true, "", routeAddressDtos, pagingMetadata);
        
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

        PagingMetadata<RouteAddress> ApplyPaging(ref IQueryable<RouteAddress> routeAddresses,
            int pageNumber, int pageSize)
        {
            var metadata = new PagingMetadata<RouteAddress>(routeAddresses,
                pageNumber, pageSize);
            
            routeAddresses = routeAddresses
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return metadata;
        }
    }
    
    public async Task<(bool isSucceed, string message, RouteAddressDto routeAddress)> GetRouteAddress(int id, string? fields)
    {
        var dbRouteAddress = await _dbContext.RouteAddresses.Where(ra => ra.Id == id)
            .FirstOrDefaultAsync();

        if (dbRouteAddress == null)
        {
            return (false, $"RouteAddress doesn't exist", null)!;
        }
        
        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = RouteAddressParameters.DefaultFields;
        }
        
        var shapedRouteAddressData = _routeAddressDataShaper.ShapeData(dbRouteAddress, fields);
        var routeAddressDto = _mapper.Map<RouteAddressDto>(shapedRouteAddressData);

        return (true, "", routeAddressDto);
    }

    public async Task<(bool isSucceed, string message, UpdateRouteAddressDto routeAddress)> UpdateRouteAddress(UpdateRouteAddressDto updateRouteAddressDto)
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
                return (false, $"RouteAddress with id:{updateRouteAddressDto.Id} doesn't exist", null)!;
            }
            
            throw;
        }

        var dbRouteAddress = await _dbContext.RouteAddresses.FirstOrDefaultAsync(ra => ra.Id == routeAddress.Id);
        
        return (true, String.Empty, _mapper.Map<UpdateRouteAddressDto>(dbRouteAddress));
    }

    public async Task<(bool isSucceed, string message)> DeleteRouteAddress(int id)
    {
        var dbRouteAddress = await _dbContext.RouteAddresses.FirstOrDefaultAsync(ra => ra.Id == id);
    
        if (dbRouteAddress == null)
        {
            return (false, $"RouteAddress with id:{id} doesn't exist");
        }
    
        _dbContext.RouteAddresses.Remove(dbRouteAddress);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty);
    }

    public async Task<bool> IsRouteAddressExists(int id)
    {
        return await _dbContext.RouteAddresses.AnyAsync(ra => ra.Id == id);
    }
}