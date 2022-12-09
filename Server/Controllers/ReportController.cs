using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("ticket")]
    public async Task<IActionResult> GetTicket(int ticketGroupId)
    {
        var result = await _reportService.GetTicket(ticketGroupId);
        
        if (!result.IsSucceed)
        {
            return BadRequest(result.actionResult);
        }

        return File(result.ticketPdf, "application/pdf",
            $"ticket.pdf");
    }
    
    [HttpGet("report")]
    public async Task<IActionResult> GetCompanyReport(int companyId, DateTime fromDate, DateTime toDate)
    {
        var result = await _reportService.GetCompanyReport(companyId, fromDate, toDate);
        
        if (!result.isSucceed)
        {
            return BadRequest(result.actionResult);
        }

        return File(result.reportPdf, "application/pdf",
            $"report.pdf");
    }
}
