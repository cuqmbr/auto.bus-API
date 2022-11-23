using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Objects;
using Route = Server.Models.Route;

namespace Server.Services;

public class RouteManagementService : IRouteManagementService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ISortHelper<Route> _routeSortHelper;
    private readonly IDataShaper<Route> _routeDataShaper;

    public RouteManagementService(ApplicationDbContext dbContext,
        IMapper mapper, ISortHelper<Route> routeSortHelper, 
        IDataShaper<Route> routeDataShaper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _routeSortHelper = routeSortHelper;
        _routeDataShaper = routeDataShaper;
    }

    public async Task<(bool isSucceed, string message, RouteDto route)> AddRoute(CreateRouteDto createRouteDto)
    {
        var route = _mapper.Map<Route>(createRouteDto);
    
        await _dbContext.Routes.AddAsync(route);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty, _mapper.Map<RouteDto>(route));
    }

    public async Task<(bool isSucceed, string message, IEnumerable<RouteDto> routes,
            PagingMetadata<Route> pagingMetadata)> GetRoutes(RouteParameters parameters)
    {
        var dbRoutes = _dbContext.Routes
            .AsQueryable();

        SearchByAllRouteFields(ref dbRoutes, parameters.Search);
        FilterByRouteType(ref dbRoutes, parameters.Type);

        try
        {
            dbRoutes = _routeSortHelper.ApplySort(dbRoutes, parameters.Sort);
            
            // By calling Any() we will check if LINQ to Entities Query will be
            // executed. If not it will throw an InvalidOperationException exception
            var isExecuted = dbRoutes.Any();
        }
        catch (Exception e)
        {
            return (false, "Invalid sorting string", null, null)!;
        }

        var pagingMetadata = ApplyPaging(ref dbRoutes, parameters.PageNumber,
            parameters.PageSize);

        var shapedRoutesData = _routeDataShaper.ShapeData(dbRoutes, parameters.Fields);
        var routeDtos = shapedRoutesData.ToList().ConvertAll(r => _mapper.Map<RouteDto>(r));
        
        return (true, "", routeDtos, pagingMetadata);

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
        

        PagingMetadata<Route> ApplyPaging(ref IQueryable<Route> routes,
            int pageNumber, int pageSize)
        {
            var metadata = new PagingMetadata<Route>(routes,
                pageNumber, pageSize);
            
            routes = routes
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return metadata;
        }
    }
    
    public async Task<(bool isSucceed, string message, RouteDto route)> GetRoute(int id, string? fields)
    {
        var dbRoute = await _dbContext.Routes.Where(r => r.Id == id)
            .FirstOrDefaultAsync();

        if (dbRoute == null)
        {
            return (false, $"Route doesn't exist", null)!;
        }
        
        if (String.IsNullOrWhiteSpace(fields))
        {
            fields = RouteParameters.DefaultFields;
        }
        
        var shapedRouteData = _routeDataShaper.ShapeData(dbRoute, fields);
        var routeDto = _mapper.Map<RouteDto>(shapedRouteData);

        return (true, "", routeDto);
    }

    public async Task<(bool isSucceed, string message, UpdateRouteDto route)> UpdateRoute(UpdateRouteDto updateRouteDto)
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
                return (false, $"Route with id:{updateRouteDto.Id} doesn't exist", null)!;
            }
            
            throw;
        }

        var dbRoute = await _dbContext.Routes.FirstOrDefaultAsync(r => r.Id == route.Id);
        
        return (true, String.Empty, _mapper.Map<UpdateRouteDto>(dbRoute));
    }

    public async Task<(bool isSucceed, string message)> DeleteRoute(int id)
    {
        var dbRoute = await _dbContext.Routes.FirstOrDefaultAsync(r => r.Id == id);
    
        if (dbRoute == null)
        {
            return (false, $"Route with id:{id} doesn't exist");
        }
    
        _dbContext.Routes.Remove(dbRoute);
        await _dbContext.SaveChangesAsync();
    
        return (true, String.Empty);
    }

    public async Task<bool> IsRouteExists(int id)
    {
        return await _dbContext.Routes.AnyAsync(r => r.Id == id);
    }
}