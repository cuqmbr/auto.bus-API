namespace SharedModels.QueryStringParameters;

public class PagingMetadata<T>
{
    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    
    public PagingMetadata(IQueryable<T> source, int pageNumber, int pageSize)
    {
        TotalCount = source.Count();
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize);
    }
}