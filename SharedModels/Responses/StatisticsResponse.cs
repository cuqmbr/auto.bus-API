namespace SharedModels.Responses;

public class StatisticsResponse
{
    public int EnrollmentsPlanned { get; set; }
    public int EnrollmentsCanceled { get; set; }
    public int TicketsSold { get; set; }
    public int TicketsReturned { get; set; }
    public double MoneyEarned { get; set; }
    public double AverageRating { get; set; }
}