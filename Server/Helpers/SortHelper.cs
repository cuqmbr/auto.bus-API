using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;

namespace Server.Helpers;

public class SortHelper<T> : ISortHelper<T>
{
    public IQueryable<T> ApplySort(IQueryable<T> entities, string? orderByQueryString)
    {
        if (!entities.Any() || String.IsNullOrWhiteSpace(orderByQueryString))
        {
            return entities;
        }

        var orderParams = orderByQueryString.Trim().Split(",");
        var propertyStrings = typeof(T) == typeof(ExpandoObject) ? 
            (entities.First() as ExpandoObject).ToDictionary(o => o.Key, o => o.Value).Keys.ToList() :
            typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList().ConvertAll(o => o.GetType().ToString());
        var orderQueryBuilder = new StringBuilder();
        
        foreach (var param in orderParams)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                continue;
            }
            
            var propertyFromQueryName = param[0] == '-' || param[0] == '+' ? param.Substring(1) : param;
            var objectProperty = propertyStrings.FirstOrDefault(ps =>
                ps.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

            if (objectProperty == null)
            {
                continue;
            }
            
            var sortingOrder = param[0] == '-' ? "descending" : "ascending";
            
            orderQueryBuilder.Append($"{objectProperty} {sortingOrder}, ");
        }

        var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');

        return typeof(T) == typeof(ExpandoObject) ? 
            entities.Cast<dynamic>().OrderBy(orderQuery).Cast<T>() : 
            entities.OrderBy(orderQuery);
    }
}