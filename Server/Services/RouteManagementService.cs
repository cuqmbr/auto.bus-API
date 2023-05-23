using System.Dynamic;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using SharedModels.DataTransferObjects.Model;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;
using Route = Server.Models.Route;

namespace Server.Services;

public class RouteManagementService : IRouteManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<ExpandoObject> _routeSortHelper;
    private readonly IDataShaper<RouteDto> _routeDataShaper;
    private readonly IDataShaper<RouteWithAddressesDto> _routeWithAddressesDataShaper;
    private readonly IPager<ExpandoObject> _pager;

    public RouteManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<ExpandoObject> routeSortHelper, 
        IDataShaper<RouteDto> routeDataShaper, 
        IDataShaper<RouteWithAddressesDto> routeWithAddressesDataShaper, 
        IPager<ExpandoObject> pager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _routeSortHelper = routeSortHelper;
        _routeDataShaper = routeDataShaper;
        _routeWithAddressesDataShaper = routeWithAddressesDataShaper;
        _pager = pager;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, RouteDto route)> AddRoute(CreateRouteDto createRouteDto)
    {
        var route = _mapper.Map<Route>(createRouteDto);
    
        await _dbContext.Routes.AddAsync(route);
        await _dbContext.SaveChangesAsync();
    
        return (true, null, _mapper.Map<RouteDto>(route));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, RouteWithAddressesDto route)> AddRouteWithAddresses(CreateRouteWithAddressesDto createRouteWithAddressesDto)
    {
        var route = _mapper.Map<Route>(createRouteWithAddressesDto);

        await _dbContext.Routes.AddAsync(route);
        await _dbContext.SaveChangesAsync();

        route = await _dbContext.Routes
            .Include(r => r.RouteAddresses).ThenInclude(ra => ra.Address)
            .ThenInclude(a => a.City).ThenInclude(c => c.State)
            .ThenInclude(s => s.Country).FirstAsync(r => r.Id == route.Id);
        
        return (true, null, _mapper.Map<RouteWithAddressesDto>(route));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> routes,
            PagingMetadata<ExpandoObject> pagingMetadata)> GetRoutes(RouteParameters parameters)
    {
        var dbRoutes = _dbContext.Routes
            .AsQueryable();

        SearchByAllRouteFields(ref dbRoutes, parameters.Search);
        FilterByRouteType(ref dbRoutes, parameters.Type);

        var routeDtos = _mapper.ProjectTo<RouteDto>(dbRoutes);
        var shapedData = _routeDataShaper.ShapeData(routeDtos, parameters.Fields).AsQueryable();

        try
        {
            shapedData = _routeSortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null!, null!);
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);
        
        return (true, null, shapedData, pagingMetadata);

        void SearchByAllRouteFields(ref IQueryable<Route> route,
            string? search)
        {
            if (!route.Any() || String.IsNullOrWhiteSpace(search))
            {
                return;
            }

            route = route.Where(c =>
                c.Type.ToLower().Contains(search.ToLower()));
        }
        
        void FilterByRouteType(ref IQueryable<Route> routes,
            string? type)
        {
            if (!routes.Any() || String.IsNullOrWhiteSpace(type))
            {
                return;
            }

            routes = routes.Where(r => r.Type == type);
        }
    }
    
    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> routes,
            PagingMetadata<ExpandoObject> pagingMetadata)> GetRoutesWithAddresses(RouteWithAddressesParameters parameters)
    {
        var dbRoutes = _dbContext.Routes
            .Include(r => r.RouteAddresses.OrderBy(ra => ra.Order))
            .ThenInclude(ra => ra.Address).ThenInclude(a => a.City)
            .ThenInclude(c => c.State).ThenInclude(s => s.Country)
            .AsQueryable();

        SearchByAllRouteFields(ref dbRoutes, parameters.Search);
        FilterByRouteType(ref dbRoutes, parameters.Type);
        FilterByFromAddressName(ref dbRoutes, parameters.FromAddressName);
        FilterByToAddressName(ref dbRoutes, parameters.ToAddressName);

        var routeDtos = _mapper.ProjectTo<RouteWithAddressesDto>(dbRoutes); 
        var shapedData = _routeWithAddressesDataShaper.ShapeData(routeDtos, parameters.Fields).AsQueryable();
        
        try
        {
            shapedData = _routeSortHelper.ApplySort(shapedData, parameters.Sort);
        }
        catch (Exception)
        {
            return (false, new BadRequestObjectResult("Invalid sorting string"), null, null)!;
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);

        return (true, null, shapedData, pagingMetadata);

        void SearchByAllRouteFields(ref IQueryable<Route> route,
            string? search)
        {
            if (!route.Any() || String.IsNullOrWhiteSpace(search))
            {
                return;
            }

            // TODO Optimize (remove client evaluation)
            route = route.ToArray().Where(r =>
                r.Type.ToLower().Contains(search.ToLower()) ||
                r.RouteAddresses.OrderBy(ra => ra.Order).First().Address
                    .GetFullName().ToLower().Contains(search.ToLower()) ||
                r.RouteAddresses.OrderBy(ra => ra.Order).Last().Address
                    .GetFullName().ToLower().Contains(search.ToLower()))
                .AsQueryable();
        }
        
        void FilterByRouteType(ref IQueryable<Route> routes,
            string? type)
        {
            if (!routes.Any() || String.IsNullOrWhiteSpace(type))
            {
                return;
            }

            routes = routes.Where(r => r.Type.ToLower().Contains(type.ToLower()));
        }
        
        void FilterByFromAddressName(ref IQueryable<Route> routes,
            string? addressName)
        {
            if (!routes.Any() || String.IsNullOrWhiteSpace(addressName))
            {
                return;
            }

            // TODO Optimize (remove client evaluation)
            routes = routes.ToArray().Where(r =>
                r.RouteAddresses.First().Address
                    .GetFullName().ToLower().Contains(addressName.ToLower()))
                .AsQueryable();
        }
        
        void FilterByToAddressName(ref IQueryable<Route> routes,
            string? addressName)
        {
            if (!routes.Any() || String.IsNullOrWhiteSpace(addressName))
            {
                return;
            }

            // TODO Optimize (remove client evaluation)
            routes = routes.ToArray().Where(r =>
                r.RouteAddresses.Last().Address.
                    GetFullName().ToLower().Contains(addressName.ToLower()))
                .AsQueryable();
        }
    }
    
    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject route)> GetRoute(int id, string? fields)
    {
        if (!await IsRouteExists(id))
        {
            return (false, new NotFoundResult(), null)!;
        }
        
        var dbRoute = await _dbContext.Routes.Where(r => r.Id == id)
            .FirstAsync();

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = RouteParameters.DefaultFields;
        }
        
        var routeDto = _mapper.Map<RouteDto>(dbRoute);
        var shapedRouteData = _routeDataShaper.ShapeData(routeDto, fields);

        return (true, null, shapedRouteData);
    }
    
    public async Task<(bool isSucceed, IActionResult? actionResult, ExpandoObject route)> GetRouteWithAddresses(int id, string? fields)
    {
        if (!await IsRouteExists(id))
        {
            return (false, new NotFoundResult(), null)!;
        }
        
        var dbRoute = await _dbContext.Routes.Where(r => r.Id == id)
            .Include(r => r.RouteAddresses).ThenInclude(ra => ra.Address)
            .ThenInclude(a => a.City).ThenInclude(c => c.State)
            .ThenInclude(s => s.Country).FirstAsync();

        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = RouteWithAddressesParameters.DefaultFields;
        }
        
        var routeDto = _mapper.Map<RouteWithAddressesDto>(dbRoute);
        var shapedData = _routeWithAddressesDataShaper.ShapeData(routeDto, fields);

        return (true, null, shapedData);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, UpdateRouteDto route)> UpdateRoute(UpdateRouteDto updateRouteDto)
    {
        var route = _mapper.Map<Route>(updateRouteDto);
        _dbContext.Entry(route).State = EntityState.Modified;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await IsRouteExists(updateRouteDto.Id))
            {
                return (false, new NotFoundResult(), null)!;
            }
        }

        var dbRoute = await _dbContext.Routes.FirstAsync(r => r.Id == route.Id);
        
        return (true, null, _mapper.Map<UpdateRouteDto>(dbRoute));
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> DeleteRoute(int id)
    {
        var dbRoute = await _dbContext.Routes.FirstOrDefaultAsync(r => r.Id == id);

        if (dbRoute == null)
        {
            return (false, new NotFoundResult());
        }
       
        _dbContext.Routes.Remove(dbRoute);
        await _dbContext.SaveChangesAsync();
    
        return (true, null);
    }

    public async Task<bool> IsRouteExists(int id)
    {
        return await _dbContext.Routes.AnyAsync(r => r.Id == id);
    }
}