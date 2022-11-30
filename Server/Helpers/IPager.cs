using SharedModels.QueryParameters;

namespace Server.Helpers;

public interface IPager<T>
{
    PagingMetadata<T> ApplyPaging(ref IQueryable<T> obj, int pageNumber, int pageSize);
}