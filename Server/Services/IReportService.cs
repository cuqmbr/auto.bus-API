namespace Server.Services;

public interface IReportService
{
    Task<(bool IsSucceed, string? message, Stream ticketPdf)> GetTicket(int ticketGroupId);

    Task<(bool isSucceed, string? message, Stream reportPdf)> GetCompanyReport();
}