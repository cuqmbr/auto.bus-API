using LiqPayIntegration;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects;

namespace Server.Services;

public class PaymentsService : IPaymentsService
{

    // LiqpayIntegration
    LiqPay liqPay = new LiqPay("sandbox_i23432845039", "sandbox_gymL9PdryqdfAznNQbb7ynLvASDQ5SJCCNJvF2iV");

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<PaymentResponse> payments)> GetPayments(DateTime from, DateTime to)
    {
        return(true, null, (await liqPay.PaymentArchive(from, to)).AsEnumerable());
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, string url)> GetPaymentUrl(PaymentDto payment)
    {
        return (true, null, (await liqPay.GetPaymentUrl(payment.Amount, payment.Description, payment.OrderId)));
    }
    
}