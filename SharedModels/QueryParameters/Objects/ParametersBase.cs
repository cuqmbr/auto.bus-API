namespace SharedModels.QueryParameters.Objects;

public class ParametersBase : PagingParameters, ISearchable, ISortable, IShapeable
{
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public string? Fields { get; set; }
}