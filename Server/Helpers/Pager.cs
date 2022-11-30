using SharedModels.QueryParameters;

namespace Server.Helpers;

public class Pager<T> : IPager<T>
{
    public PagingMetadata<T> ApplyPaging(ref IQueryable<T> obj,
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