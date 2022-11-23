namespace SharedModels.QueryParameters.Objects;

public class VehicleParameters : ParametersBase
{
    public const string DefaultFields = "id,companyId,number,type,capacity,hasClimateControl," +
                                        "hasWiFi,hasWC,hasStewardess,hasTV,hasOutlet,hasBelts";
    
    public VehicleParameters()
    {
        Sort = "id";
        Fields = DefaultFields;
    }
    
    public string? Number { get; set; }
    public string? Type { get; set; }
    public int? FromCapacity { get; set; }
    public int? ToCapacity { get; set; }

    public bool? HasClimateControl { get; set; }
    public bool? HasWiFi { get; set; }
    public bool? HasWC { get; set; }
    public bool? HasStewardess { get; set; }
    public bool? HasTV { get; set; }
    public bool? HasOutlet { get; set; }
    public bool? HasBelts { get; set; }
    
    public int? CompanyId { get; set; }
}