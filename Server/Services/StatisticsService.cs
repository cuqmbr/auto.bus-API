using System.Dynamic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Helpers;
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
    private readonly IDataShaper<ExpandoObject> _expandoDataShaper;
    private readonly IPager<ExpandoObject> _pager;
    private readonly ISortHelper<ExpandoObject> _sortHelper;

    public StatisticsService(ApplicationDbContext dbContext, IMapper mapper,
        IDataShaper<UserDto> userDataShaper, IDataShaper<CompanyDto> companyDataShaper, 
        IDataShaper<AddressDto> addressDataShaper, IPager<ExpandoObject> pager, 
        IDataShaper<ExpandoObject> expandoDataShaper, ISortHelper<ExpandoObject> sortHelper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _userDataShaper = userDataShaper;
        _companyDataShaper = companyDataShaper;
        _addressDataShaper = addressDataShaper;
        _pager = pager;
        _expandoDataShaper = expandoDataShaper;
        _sortHelper = sortHelper;
    }

    // Popularity is measured in number of purchased tickets
    public async Task<(bool IsSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> route,
        PagingMetadata<ExpandoObject> pagingMetadata)> 
        GetPopularRoutes(PopularRoutesParameters parameters)
    {
        parameters.Days ??= parameters.DefaultDays;
        var fromDateUtc = DateTime.UtcNow.Date - TimeSpan.FromDays((double) parameters.Days);

        var dbTicketGroupsArray = await _dbContext.TicketGroups
            .Include(tg => tg.Tickets)
            .Where(tg => tg.PurchaseDateTimeUtc >= fromDateUtc)
            .ToArrayAsync();
        
        var depArrCombCountDict = new Dictionary<(int, int), int>();

        foreach (var tg in dbTicketGroupsArray)
        {
            // TODO: implement ticket ordering
            var departureAddress = tg.Tickets.OrderBy(t => t.Id).First().FirstRouteAddressId;
            var arrivalAddress = tg.Tickets.OrderBy(t => t.Id).Last().LastRouteAddressId;
            
            if (!depArrCombCountDict.ContainsKey((departureAddress, arrivalAddress)))
            {
                depArrCombCountDict.Add((departureAddress, arrivalAddress), 1);
            }
            else
            {
                depArrCombCountDict[(departureAddress, arrivalAddress)] += 1;
            }
        }

        depArrCombCountDict = depArrCombCountDict
            .OrderByDescending(a => a.Value)
            .Take(parameters.Amount)
            .ToDictionary(x => x.Key, x => x.Value);

        var addressIds = new List<int>();
        foreach (var depArrAddressIds in depArrCombCountDict.Keys)
        {
            var (departureAddressId, arrivalAddressId) = depArrAddressIds;

            if (!addressIds.Contains(departureAddressId))
            {
                addressIds.Add(departureAddressId);
            }
            
            if (!addressIds.Contains(arrivalAddressId))
            {
                addressIds.Add(arrivalAddressId);
            }
        }

        var dbAddressArray = await _dbContext.Addresses
            .Include(a => a.City)
            .ThenInclude(c => c.State)
            .ThenInclude(s => s.Country)
            .Where(a => addressIds.Contains(a.Id))
            .ToArrayAsync();

        var derArrCount = new List<ExpandoObject>();

        foreach (var depArrCount in depArrCombCountDict)
        {
            var obj = new ExpandoObject();

            var departureAddressId = depArrCount.Key.Item1;
            var departureAddress = dbAddressArray
                .First(a => a.Id == departureAddressId);
            var arrivalAddressId = depArrCount.Key.Item2;
            var arrivalAddress = dbAddressArray
                .First(a => a.Id == arrivalAddressId);
            var count = depArrCount.Value;

            var fields = parameters.Fields!.Split(',');
            
            if (fields.Any(f => f.ToLower() == "departureAddressId".ToLower()))
                obj.TryAdd("departureAddressId", departureAddressId);
            if (fields.Any(f => f.ToLower() == "departureAddress".ToLower()))
                obj.TryAdd("departureAddress", departureAddress.GetFullName());
            if (fields.Any(f => f.ToLower() == "arrivalAddressId".ToLower()))
                obj.TryAdd("arrivalAddressId", arrivalAddressId);
            if (fields.Any(f => f.ToLower() == "arrivalAddress".ToLower()))
                obj.TryAdd("arrivalAddress", arrivalAddress.GetFullName());
            if (fields.Any(f => f.ToLower() == "count".ToLower()))
                obj.TryAdd("count", count);
            
            derArrCount.Add(obj);
        }

        var n = derArrCount.Count;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if ((int) (derArrCount[j] as IDictionary<string, object>)["count"]  > (int) (derArrCount[j + 1] as IDictionary<string, object>)["count"]) 
                {   
                    // swap temp and arr[i]
                    (derArrCount[j], derArrCount[j + 1]) = (derArrCount[j + 1], derArrCount[j]);
                    (derArrCount[j], derArrCount[j + 1]) = (derArrCount[j + 1], derArrCount[j]);
                }
            }
        }
        
        var result = derArrCount.AsQueryable();
        
        var pagingMetadata = _pager.ApplyPaging(ref result,
            parameters.PageNumber, parameters.PageSize);
        
        return (true, null, result.AsEnumerable(), pagingMetadata);
    }

    // Engagement is measured in number of purchases made in past N days
    // One purchase contains one (direct route) or more (route with transfers) tickets 
    public async Task<(bool IsSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> users, 
            PagingMetadata<ExpandoObject> pagingMetadata)>
        GetEngagedUsers(EngagedUserParameters parameters)
    {
        parameters.Days ??= parameters.DefaultDays;
        var fromDateUtc = DateTime.UtcNow.Date - TimeSpan.FromDays((double) parameters.Days);

        var dbUsers = _dbContext.Users
            .Include(u => u.TicketGroups)
            .ThenInclude(tg => tg.Tickets)
            .Select(u => new
            {
                User = u,
                TicketGroups = u.TicketGroups.Where(tg =>
                    tg.PurchaseDateTimeUtc >= fromDateUtc)
            })
            .OrderByDescending(o => o.TicketGroups.Count())
            .Take(parameters.Amount)
            .Select(i => i.User);
        
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
    public async Task<(bool IsSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> companies, 
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
            double result = 0;
            int reviewCount = 0;
            
            foreach (var v in dbCompaniesArray[i].Vehicles)
            {
                foreach (var ve in v.VehicleEnrollments)
                {
                    foreach (var r in ve.Reviews)
                    {
                        result += r.Rating;
                        reviewCount++;
                    }
                }
            }

            companiesAvgRatings[i] = reviewCount != 0 ? Math.Round(result / reviewCount, 5) : 0;
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
    public async Task<(bool IsSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> stations, 
            PagingMetadata<ExpandoObject> pagingMetadata)>
        GetPopularStations(PopularAddressesParameters parameters)
    {
        parameters.Days ??= parameters.DefaultDays;
        var fromDateUtc = DateTime.UtcNow.Date - TimeSpan.FromDays((double) parameters.Days);

        var dbTicketGroupsArray = await _dbContext.TicketGroups
            .Include(tg => tg.Tickets)
            .Where(tg => tg.PurchaseDateTimeUtc >= fromDateUtc)
            .ToArrayAsync();

        // Count appearances for each address id <Id, Count> 
        var addressCountDict = new Dictionary<int, int>();

        foreach (var tg in dbTicketGroupsArray)
        {
            // TODO: implement ticket ordering
            var tickets = tg.Tickets.OrderBy(t => t.Id).ToArray();
            
            if (!addressCountDict.ContainsKey(tickets.First().FirstRouteAddressId))
            {
                addressCountDict.Add(tickets.First().FirstRouteAddressId, 1);
            }
            else
            {
                addressCountDict[tickets.First().FirstRouteAddressId] += 1;
            }
            
            if (!addressCountDict.ContainsKey(tickets.Last().LastRouteAddressId))
            {
                addressCountDict.Add(tickets.Last().LastRouteAddressId, 1);
            }
            else
            {
                addressCountDict[tickets.Last().LastRouteAddressId] += 1;
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
            .Include(a => a.City)
            .ThenInclude(c => c.State)
            .ThenInclude(s => s.Country)
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
                    (addressCountArray[j], addressCountArray[j + 1]) = 
                        (addressCountArray[j + 1], addressCountArray[j]);
                    (dbAddressesArray[j], dbAddressesArray[j + 1]) = 
                        (dbAddressesArray[j + 1], dbAddressesArray[j]);
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