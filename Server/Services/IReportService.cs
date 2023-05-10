using Microsoft.AspNetCore.Mvc;
using SharedModels.Responses;

namespace Server.Services;

public interface IReportService
{
    Task<(bool IsSucceed, IActionResult? actionResult, Stream ticketPdf)> 
        GetTicket(int ticketGroupId);

    Task<(bool isSucceed, IActionResult? actionResult, Stream reportPdf)> 
        GetCompanyReportPdf(int companyId, DateTime fromDate, DateTime toDate);
    
    Task<(bool isSucceed, IActionResult? actionResult, StatisticsResponse statistics)> 
        GetCompanyReportRaw(int companyId, DateTime fromDate, DateTime toDate);
    
    Task<(bool isSucceed, IActionResult? actionResult, StatisticsResponse statistics)> 
        GetAdminReportRaw(DateTime fromDate, DateTime toDate);
}