namespace SharedModels.QueryParameters.Statistics;

public class ParametersBase : PagingParameters, IShapeable
{
    public int Amount { get; set; } = 10;
    public string? Fields { get; set; }
}