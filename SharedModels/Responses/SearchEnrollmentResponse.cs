namespace SharedModels.Responses;

public class SearchEnrollmentResponse
{
    public SearchEnrollmentResponse()
    {
        EnrollmentGroups = new List<EnrollmentGroup>();
    }
    
    public IList<EnrollmentGroup> EnrollmentGroups { get; set; }
}

public class EnrollmentGroup
{
    public EnrollmentGroup()
    {
        Enrollments = new List<FlattenedEnrollment>();
    }
    
    public IList<FlattenedEnrollment> Enrollments { get; set; }
    public TimeSpan Duration { get; set; }
    public double Cost { get; set; }
}

public class FlattenedEnrollment
{
    public int Id { get; set; }
    
    public int DepartureAddressId { get; set; }
    public DateTime DepartureTime { get; set; }
    public string DepartureAddressName { get; set; } = null!;
    public string DepartureCityName { get; set; } = null!;
    public string DepartureStateName { get; set; } = null!;
    public string DepartureCountryName { get; set; } = null!;
    public string DepartureAddressFullName { get; set; } = null!;
    
    public int ArrivalAddressId { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string ArrivalAddressName { get; set; } = null!;
    public string ArrivalCityName { get; set; } = null!;
    public string ArrivalStateName { get; set; } = null!;
    public string ArrivalCountryName { get; set; } = null!;
    public string ArrivalAddressFullName { get; set; } = null!;
    
    public int Order { get; set; }

    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = null!;

    public int VehicleId { get; set; }
    public string VehicleType { get; set; } = null!;
    public string VehicleNumber { get; set; } = null!;
}