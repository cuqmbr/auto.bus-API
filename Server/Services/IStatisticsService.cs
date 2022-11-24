using System.Dynamic;
using Server.Models;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Statistics;

namespace Server.Services;

public interface IStatisticsService
{
    Task<(bool IsSucceed, string? message, IEnumerable<ExpandoObject> route)>
        GetPopularRoutes(int amount);

    Task<(bool IsSucceed, string? message, IEnumerable<ExpandoObject> users, PagingMetadata<User> pagingMetadata)>
        GetEngagedUsers(EngagedUserParameters parameters);

    Task<(bool IsSucceed, string? message, IEnumerable<ExpandoObject> companies, PagingMetadata<Company> pagingMetadata)>
        GetPopularCompanies(PopularCompanyParameters parameters);

    Task<(bool IsSucceed, string? message, IEnumerable<ExpandoObject> stations)>
        GetPopularStations(int amount);
}