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

    public StatisticsService(ApplicationDbContext dbContext, IMapper mapper,
        IDataShaper<UserDto> userDataShaper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _userDataShaper = userDataShaper;
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
        
        var resultObjectList = _dbContext.Users
            .Include(u => u.Tickets)
            .Select(u => new
            {
                User = u,
                Tickets = u.Tickets.Where(t => t.PurchaseDateTimeUtc >= fromDateUtc)
            })
            .OrderByDescending(o => o.User.Tickets.Count)
            .Take(parameters.Amount);

        
        var dbUsers = resultObjectList.Select(i => i.User);
        var pagingMetadata = ApplyPaging(ref dbUsers, parameters.PageNumber,
            parameters.PageSize);
        
        var userDtos = _mapper.ProjectTo<UserDto>(dbUsers).ToList();
        var shapedData = _userDataShaper
            .ShapeData(userDtos, parameters.Fields ?? parameters.DefaultFields)
            .ToList();

        if (parameters.Fields != null &&
            parameters.Fields.ToLower().Contains("ticketCount".ToLower()))
        {
            var dbUsersList = await dbUsers.ToListAsync();
            for (int i = 0; i < dbUsersList.Count; i++)
            {
                var ticketCount = dbUsersList[i].Tickets.Count;
                shapedData[i].TryAdd("TicketCount", ticketCount);
            }
        }
        
        return (true, null, shapedData, pagingMetadata);
    }

    // Popularity is measured in number of purchased tickets & rating
    public async Task<(bool IsSucceed, string? message, IEnumerable<ExpandoObject> companies)> 
        GetPopularCompanies(int amount)
    {
        throw new NotImplementedException();
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