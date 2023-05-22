using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("pdf/ticket")]
    public async Task<IActionResult> GetTicketPdf(int ticketGroupId)
    {
        var result = await _reportService.GetTicket(ticketGroupId);
        
        if (!result.IsSucceed)
        {
            return result.actionResult;
        }

        return File(result.ticketPdf, "application/pdf", $"ticket.pdf");
    }
    
    [Authorize(Policy = "CompanyAccess")]
    [HttpGet("pdf/company")]
    public async Task<IActionResult> GetCompanyReportPdf(int? companyId, DateTime fromDate, DateTime toDate)
    {
        var result = await _reportService.GetCompanyReportPdf(companyId, fromDate, toDate);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return File(result.reportPdf, "application/pdf", $"report.pdf");
    }
    
    [Authorize(Policy = "CompanyAccess")]
    [HttpGet("raw/company")]
    public async Task<IActionResult> GetCompanyReportRaw(int? companyId, DateTime fromDate, DateTime toDate)
    {
        var result = await _reportService.GetCompanyReportRaw(companyId, fromDate, toDate);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.statistics);
    }
    
    [Authorize(Policy = "AdministratorAccess")]
    [HttpGet("raw/admin")]
    public async Task<IActionResult> GetAdminReportRaw(DateTime fromDate, DateTime toDate)
    {
        var result = await _reportService.GetAdminReportRaw(fromDate, toDate);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.statistics);
    }
}
