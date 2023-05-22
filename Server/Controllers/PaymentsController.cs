using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;
using SharedModels.Responses;

namespace Server.Controllers;

[Route("api/payment")]
[ApiController]
public class PaymentController : ControllerBase
{
    readonly IPaymentsService _paymentsService;

    public PaymentController(IPaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
    }

    [HttpPost("link")]
    public async Task<IActionResult?> GetPaymentLink([FromBody] IList<StrippedFlattenedEnrollment> input)
    {
        var result = await _paymentsService.GetPaymentUrl(input);

        return Ok(result.url);
    }

    [HttpPost("callback")]
    public async Task<IActionResult> ReceiveCallback()
    {
        if (HttpContext.Request.ContentType != "application/x-www-form-urlencoded")
            return BadRequest();

        var result = await _paymentsService.ReceiveCallback(HttpContext.Request.Form);
        
        return result.actionResult;
    }
}
