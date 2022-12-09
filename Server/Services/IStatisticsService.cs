using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using SharedModels.QueryParameters;
using SharedModels.QueryParameters.Statistics;

namespace Server.Services;

public interface IStatisticsService
{
    Task<(bool IsSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> route,
        PagingMetadata<ExpandoObject> pagingMetadata)>
        GetPopularRoutes(PopularRoutesParameters parameters);

    Task<(bool IsSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> users, 
            PagingMetadata<ExpandoObject> pagingMetadata)>
        GetEngagedUsers(EngagedUserParameters parameters);

    Task<(bool IsSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> companies, 
            PagingMetadata<ExpandoObject> pagingMetadata)>
        GetPopularCompanies(PopularCompanyParameters parameters);

    Task<(bool IsSucceed, IActionResult? actionResult, IEnumerable<ExpandoObject> stations, 
            PagingMetadata<ExpandoObject> pagingMetadata)>
        GetPopularStations(PopularAddressesParameters parameters);
}