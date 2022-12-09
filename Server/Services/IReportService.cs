using Microsoft.AspNetCore.Mvc;

namespace Server.Services;

public interface IReportService
{
    Task<(bool IsSucceed, IActionResult? actionResult, Stream ticketPdf)> 
        GetTicket(int ticketGroupId);

    Task<(bool isSucceed, IActionResult? actionResult, Stream reportPdf)> 
        GetCompanyReport(int companyId, DateTime fromDate, DateTime toDate);
}