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
            return BadRequest(result.message);
        }

        return File(result.ticketPdf, "application/pdf",
            $"ticket.pdf");
    }
}
