using Microsoft.AspNetCore.Mvc;
using Server.Services;
using SharedModels.DataTransferObjects;

[Route("api/payment")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IPaymentsService _paymentsService;
    public PaymentController(IPaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
    }

   [HttpGet]
    public async Task<IActionResult> GetPaymentLink([FromQuery] PaymentDto payment)
    {
        var result = await _paymentsService.GetPaymentUrl(payment);

        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok(result.url);
    }
}
