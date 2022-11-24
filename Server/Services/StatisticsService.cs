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

    public StatisticsService(ApplicationDbContext dbContext, IMapper mapper,
        IDataShaper<UserDto> userDataShaper, IDataShaper<CompanyDto> companyDataShaper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _userDataShaper = userDataShaper;
        _companyDataShaper = companyDataShaper;
    }

    // Popularity is measured in number of purchased tickets
    public async Task<(bool IsSucceed, string? message, IEnumerable<ExpandoObject> route)> 
        GetPopularRoutes(int amount)
    {
        throw new NotImplementedException();
    }

    // Engagement is measured in number of tickets bought in last 60 days
    public async Task<(bool IsSucceed, string? message, IEnumerable<ExpandoObject> users, PagingMetadata<User> pagingMetadata)> 
        GetEngagedUsers(EngagedUserParameters parameters)
    {
        var fromDateUtc = DateTime.UtcNow - TimeSpan.FromDays(parameters.Days ?? parameters.DefaultDays);
        
        var resultObjects = _dbContext.Users
            .Include(u => u.Tickets)
            .Select(u => new
            {
                User = u,
                Tickets = u.Tickets.Where(t => t.PurchaseDateTimeUtc >= fromDateUtc)
            })
            .OrderByDescending(o => o.User.Tickets.Count)
            .Take(parameters.Amount);

        
        var dbUsers = resultObjects.Select(i => i.User);
        var pagingMetadata = ApplyPaging(ref dbUsers, parameters.PageNumber,
            parameters.PageSize);
        
        var userDtos = _mapper.ProjectTo<UserDto>(dbUsers).ToArray();
        var shapedData = _userDataShaper
            .ShapeData(userDtos, parameters.Fields ?? parameters.DefaultFields)
            .ToArray();

        if (parameters.Fields != null &&
            parameters.Fields.ToLower().Contains("ticketCount".ToLower()))
        {
            var dbUsersArray = await dbUsers.ToArrayAsync();
            for (int i = 0; i < dbUsersArray.Length; i++)
            {
                var ticketCount = dbUsersArray[i].Tickets.Count;
                shapedData[i].TryAdd("TicketCount", ticketCount);
            }
        }
        
        return (true, null, shapedData, pagingMetadata);
    }

    // Popularity is measured in average rating of all VehicleEnrollments of a company
    public async Task<(bool IsSucceed, string? message, IEnumerable<ExpandoObject> companies, PagingMetadata<Company> pagingMetadata)> 
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

        companiesAvgRatings = companiesAvgRatings.Skip(companiesAvgRatings.Length - parameters.Amount).Reverse().ToArray();
        var popularCompanies = dbCompaniesArray.Skip(companiesAvgRatings.Length - parameters.Amount).Reverse().AsQueryable();
        
        // Apply paging, convert to DTOs and shape data
        
        var pagingMetadata = ApplyPaging(ref popularCompanies, parameters.PageNumber, parameters.PageSize);
        var companyDtos = _mapper.ProjectTo<CompanyDto>(popularCompanies).ToArray();

        var shapedData = _companyDataShaper.ShapeData(companyDtos, parameters.Fields ?? parameters.DefaultFields).ToArray();

        if (parameters.Fields != null &&
            parameters.Fields.ToLower().Contains("rating".ToLower()))
        {
            for (int i = 0; i < shapedData.Length; i++)
            {
                shapedData[i].TryAdd("Rating", companiesAvgRatings[i]);
            }
        }
        
        return (true, null, shapedData, pagingMetadata);
    }

    // Popularity is measured in number of routes using the station
    public async Task<(bool IsSucceed, string? message, IEnumerable<ExpandoObject> stations)> 
        GetPopularStations(int amount)
    {
        throw new NotImplementedException();
    }
    
    PagingMetadata<T> ApplyPaging<T>(ref IQueryable<T> obj,
        int pageNumber, int pageSize)
    {
        var metadata = new PagingMetadata<T>(obj,
            pageNumber, pageSize);
            
        obj = obj
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        return metadata;
    }
}