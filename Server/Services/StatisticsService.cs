using System.Dynamic;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
using Server.Models;
using SharedModels.DataTransferObjects;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Statistics;

namespace Server.Services;

public class StatisticsService : IStatisticsService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IDataShaper<UserDto> _userDataShaper;
    private readonly IDataShaper<CompanyDto> _companyDataShaper;
    private readonly IDataShaper<AddressDto> _addressDataShaper;
    private readonly IPager<ExpandoObject> _pager;

    public StatisticsService(ApplicationDbContext dbContext, IMapper mapper,
        IDataShaper<UserDto> userDataShaper, IDataShaper<CompanyDto> companyDataShaper, 
        IDataShaper<AddressDto> addressDataShaper, IPager<ExpandoObject> pager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _userDataShaper = userDataShaper;
        _companyDataShaper = companyDataShaper;
        _addressDataShaper = addressDataShaper;
        _pager = pager;
    }

    // Popularity is measured in number of purchased tickets
    public async Task<(bool IsSucceed, string? message, IEnumerable<ExpandoObject> route)> 
        GetPopularRoutes(int amount)
    {
        throw new NotImplementedException();
    }

    // Engagement is measured in number of purchases made in past N days
    // One purchase contains one (direct route) or more (route with transfers) tickets 
    public async Task<(bool IsSucceed, string? message, IEnumerable<ExpandoObject> users, 
            PagingMetadata<ExpandoObject> pagingMetadata)>
        GetEngagedUsers(EngagedUserParameters parameters)
    {
        var fromDateUtc = 
            DateTime.UtcNow - TimeSpan.FromDays(parameters.Days ?? parameters.DefaultDays);

        var resultObjects = _dbContext.Users
            .Include(u => u.TicketGroups)
            .ThenInclude(tg => tg.Tickets)
            .Select(u => new
            {
                User = u,
                TicketGroups = u.TicketGroups.Where(tg =>
                    tg.Tickets.First().PurchaseDateTimeUtc >= fromDateUtc)
            })
            .OrderByDescending(o => o.TicketGroups.Count())
            .Take(parameters.Amount);

        
        var dbUsers = resultObjects.Select(i => i.User);
        var userDtos = _mapper.ProjectTo<UserDto>(dbUsers).ToArray();
        var shapedDataArray = _userDataShaper
            .ShapeData(userDtos, parameters.Fields ?? parameters.DefaultFields)
            .ToArray();
        
        if (parameters.Fields != null &&
            parameters.Fields.ToLower().Contains("purchaseCount".ToLower()))
        {
            var dbUsersArray = await dbUsers.ToArrayAsync();
            for (int i = 0; i < dbUsersArray.Length; i++)
            {
                var ticketCount = dbUsersArray[i].TicketGroups.Count;
                shapedDataArray[i].TryAdd("PurchaseCount", ticketCount);
            }
        }
        
        var shapedData = shapedDataArray.AsQueryable();
        var pagingMetadata = _pager.ApplyPaging(ref shapedData,
            parameters.PageNumber, parameters.PageSize);
        shapedDataArray = shapedData.ToArray();

        return (true, null, shapedDataArray, pagingMetadata);
    }

    // Popularity is measured in average rating of all VehicleEnrollments of a company
    public async Task<(bool IsSucceed, string? message, IEnumerable<ExpandoObject> companies, 
            PagingMetadata<ExpandoObject> pagingMetadata)>
        GetPopularCompanies(PopularCompanyParameters parameters)
    {
        var dbCompanies = _dbContext.Companies
            .Include(c => c.Vehicles)
            .ThenInclude(v => v.VehicleEnrollments)
            .ThenInclude(ve => ve.Tickets)
            .Include(c => c.Vehicles)
            .ThenInclude(v => v.VehicleEnrollments)
            .ThenInclude(ve => ve.Reviews);

        // Calculate average rating for each company
        
        var dbCompaniesArray = await dbCompanies.ToArrayAsync();
        double[] companiesAvgRatings = new double[dbCompaniesArray.Length];

        for (int i = 0; i < dbCompaniesArray.Length; i++)
        {
            double tempC = 0;
            
            foreach (var v in dbCompaniesArray[i].Vehicles)
            {
                double tempV = 0;
                
                foreach (var ve in v.VehicleEnrollments)
                {
                    double tempVE = 0;
                    
                    foreach (var r in ve.Reviews)
                    {
                        tempVE += r.Rating;
                    }

                    tempV += tempVE / ve.Reviews.Count;
                }

                tempC += tempV / v.VehicleEnrollments.Count;
            }

            companiesAvgRatings[i] = tempC / dbCompaniesArray[i].Vehicles.Count;
        }

        // Sort companiesAvgRatings and apply the same sorting to dbCompaniesArray
        
        int n = companiesAvgRatings.Length;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (companiesAvgRatings[j] > companiesAvgRatings[j + 1]) 
                {
                    // swap temp and arr[i]
                    (companiesAvgRatings[j], companiesAvgRatings[j + 1]) = (companiesAvgRatings[j + 1], companiesAvgRatings[j]);
                    (dbCompaniesArray[j], dbCompaniesArray[j + 1]) = (dbCompaniesArray[j + 1], dbCompaniesArray[j]);
                }
            }
        }

        companiesAvgRatings = companiesAvgRatings
            .Skip(companiesAvgRatings.Length - parameters.Amount).Reverse()
            .ToArray();
        var popularCompanies = dbCompaniesArray
            .Skip(companiesAvgRatings.Length - parameters.Amount).Reverse()
            .AsQueryable();
        
        // Convert to DTOs, shape data and apply paging

        var companyDtos = _mapper.ProjectTo<CompanyDto>(popularCompanies);
        var shapedDataArray = _companyDataShaper.ShapeData(companyDtos,
            parameters.Fields ?? parameters.DefaultFields).ToArray();

        if (parameters.Fields != null &&
            parameters.Fields.ToLower().Contains("rating".ToLower()))
        {
            for (int i = 0; i < shapedDataArray.Length; i++)
            {
                shapedDataArray[i].TryAdd("Rating", companiesAvgRatings[i]);
            }
        }
        
        var shapedData = shapedDataArray.AsQueryable();
        var pagingMetadata = _pager.ApplyPaging(ref shapedData,
            parameters.PageNumber, parameters.PageSize);
        shapedDataArray = shapedData.ToArray();
        
        return (true, null, shapedDataArray, pagingMetadata);
    }

    // Popularity is measured in number tickets in which the address is the first or last station
    public async Task<(bool IsSucceed, string? message, IEnumerable<ExpandoObject> stations, 
            PagingMetadata<ExpandoObject> pagingMetadata)>
        GetPopularStations(PopularAddressesParameters parameters)
    {
        // throw new NotImplementedException();

        var fromDateUtc = 
            DateTime.UtcNow - TimeSpan.FromDays(parameters.Days ?? parameters.DefaultDays);

        var dbTicketGroupsArray = await _dbContext.TicketGroups
            .Include(tg => tg.Tickets)
            .Where(tg => tg.Tickets.First().PurchaseDateTimeUtc >= fromDateUtc)
            .ToArrayAsync();

        // Count appearances for each address id <Id, Count> 
        var addressCountDict = new Dictionary<int, int>();

        foreach (var tg in dbTicketGroupsArray)
        {
            if (!addressCountDict.ContainsKey(tg.Tickets.First().FirstRouteAddressId))
            {
                addressCountDict.Add(tg.Tickets.First().FirstRouteAddressId, 1);
            }
            else
            {
                addressCountDict[tg.Tickets.First().FirstRouteAddressId] += 1;
            }
            
            if (!addressCountDict.ContainsKey(tg.Tickets.Last().LastRouteAddressId))
            {
                addressCountDict.Add(tg.Tickets.Last().LastRouteAddressId, 1);
            }
            else
            {
                addressCountDict[tg.Tickets.Last().LastRouteAddressId] += 1;
            }
        }

        // Sort by number of appearances in descending order ->
        // Take amount given in parameters ->
        // Order by Id in Ascending order (needed for further sorting of two arrays simultaneously)
        addressCountDict = addressCountDict.OrderByDescending(a => a.Value)
            .Take(parameters.Amount).OrderBy(a => a.Key)
            .ToDictionary(x => x.Key, x => x.Value);
        
        // Separate Ids and counts into two arrays
        var addressIds = addressCountDict.Keys.ToArray();
        var addressCountArray = addressCountDict.Values.ToArray();
        
        // Get top addresses from database ordered by Id (same as
        // addressIds addressCountDict and )
        var dbAddressesArray =  await _dbContext.Addresses
            .Where(a => addressIds.Any(id => a.Id == id))
            .OrderBy(a => a.Id).ToArrayAsync();

        // Sort addressCountArray and simultaneously sort dbAddressesArray
        // in the same manner
        int n = addressCountArray.Length;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (addressCountArray[j] > addressCountArray[j + 1]) 
                {
                    // swap temp and arr[i]
                    (addressCountArray[j], addressCountArray[j + 1]) = (addressCountArray[j + 1], addressCountArray[j]);
                    (dbAddressesArray[j], dbAddressesArray[j + 1]) = (dbAddressesArray[j + 1], dbAddressesArray[j]);
                }
            }
        }

        // Reverse sorted arrays (the result will be two "linked" arrays sorterd
        // in descending order by addressCount)
        addressCountArray = addressCountArray.Reverse().ToArray();
        dbAddressesArray = dbAddressesArray.Reverse().ToArray();

        var addressDtos =
            _mapper.ProjectTo<AddressDto>(dbAddressesArray.AsQueryable());
        var shapedDataArray = _addressDataShaper.ShapeData(addressDtos,
            parameters.Fields ?? parameters.DefaultFields).ToArray();

        if (parameters.Fields != null &&
            parameters.Fields.ToLower().Contains("purchaseCount".ToLower()))
        {
            for (int i = 0; i < shapedDataArray.Length; i++)
            {
                shapedDataArray[i].TryAdd("purchaseCount", addressCountArray[i]);
            }
        }

        var shapedData = shapedDataArray.AsQueryable();
        var pagingMetadata = _pager.ApplyPaging(ref shapedData,
            parameters.PageNumber, parameters.PageSize);
        shapedDataArray = shapedData.ToArray();
        
        return (true, null, shapedDataArray, pagingMetadata);
    }
}