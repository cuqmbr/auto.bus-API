using System.Dynamic;
using AutoMapper;
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
    private readonly IPager<ExpandoObject> _pager;

    public RouteManagementService(ApplicationDbContext dbContext, IMapper mapper,
        ISortHelper<ExpandoObject> routeSortHelper, IDataShaper<RouteDto> routeDataShaper,
        IPager<ExpandoObject> pager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _routeSortHelper = routeSortHelper;
        _routeDataShaper = routeDataShaper;
        _pager = pager;
    }

    public async Task<(bool isSucceed, IActionResult actionResult, RouteDto route)>
        AddRoute(CreateRouteDto createRouteDto)
    {
        var route = _mapper.Map<Route>(createRouteDto);

        await _dbContext.Routes.AddAsync(route);
        await _dbContext.SaveChangesAsync();

        route = await _dbContext.Routes
            .Include(r => r.RouteAddresses).ThenInclude(ra => ra.Address)
            .ThenInclude(a => a.City).ThenInclude(c => c.State)
            .ThenInclude(s => s.Country).FirstAsync(r => r.Id == route.Id);
        
        return (true, null!, _mapper.Map<RouteDto>(route));
    }

    public async Task<(bool isSucceed, IActionResult actionResult, IEnumerable<ExpandoObject> routes, PagingMetadata<ExpandoObject> pagingMetadata)>
        GetRoutes(RouteParameters parameters)
    {
        var dbRoutes = _dbContext.Routes
            .Include(r => r.RouteAddresses.OrderBy(ra => ra.Order))
            .ThenInclude(ra => ra.Address).ThenInclude(a => a.City)
            .ThenInclude(c => c.State).ThenInclude(s => s.Country)
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
            return (false, new BadRequestObjectResult("Invalid sorting string"), null, null)!;
        }
        
        var pagingMetadata = _pager.ApplyPaging(ref shapedData, parameters.PageNumber,
            parameters.PageSize);

        return (true, null!, shapedData, pagingMetadata);

        void SearchByAllRouteFields(ref IQueryable<Route> route, string? search)
        {
            if (!route.Any() || String.IsNullOrWhiteSpace(search))
            {
                return;
            }

            // TODO Optimize (remove client evaluation)
            route = route.ToArray().Where(r =>
                r.Type.ToLower().Contains(search.ToLower()) ||
                r.RouteAddresses.Any(ra => ra.Address.GetFullName().ToLower().Contains(search.ToLower())))
                .AsQueryable();
        }
        
        void FilterByRouteType(ref IQueryable<Route> routes, string? type)
        {
            if (!routes.Any() || String.IsNullOrWhiteSpace(type))
            {
                return;
            }

            routes = routes.Where(r => r.Type.ToLower().Contains(type.ToLower()));
        }
    }
    
    public async Task<(bool isSucceed, IActionResult actionResult, ExpandoObject route)>
        GetRoute(int id, string? fields)
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
            fields = RouteParameters.DefaultFields;
        }
        
        var routeDto = _mapper.Map<RouteDto>(dbRoute);
        var shapedData = _routeDataShaper.ShapeData(routeDto, fields);

        return (true, null!, shapedData);
    }

    public async Task<(bool isSucceed, IActionResult actionResult, UpdateRouteDto route)>
        UpdateRoute(int id, UpdateRouteDto updateRouteDto)
    {
        if (id != updateRouteDto.Id)
        {
            return (false, new BadRequestObjectResult("Query id must match object id"), null!);
        }
        
        var route = _mapper.Map<Route>(updateRouteDto);
        _dbContext.Routes.Update(route);
        
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

        var dbRoute = await _dbContext.Routes
            .Include(r => r.RouteAddresses).ThenInclude(ra => ra.Address)
            .ThenInclude(a => a.City).ThenInclude(c => c.State)
            .ThenInclude(s => s.Country).FirstAsync(r => r.Id == route.Id);
        
        return (true, null!, _mapper.Map<UpdateRouteDto>(dbRoute));
    }

    public async Task<(bool isSucceed, IActionResult actionResult)> DeleteRoute(int id)
    {
        var dbRoute = await _dbContext.Routes.FirstOrDefaultAsync(r => r.Id == id);

        if (dbRoute == null)
        {
            return (false, new NotFoundResult());
        }
       
        _dbContext.Routes.Remove(dbRoute);
        await _dbContext.SaveChangesAsync();
    
        return (true, null!);
    }

    private async Task<bool> IsRouteExists(int id)
    {
        return await _dbContext.Routes.AnyAsync(r => r.Id == id);
    }
}