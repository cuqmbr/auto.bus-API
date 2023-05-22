using LiqPayIntegration;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Responses;

namespace Server.Services;

public interface IPaymentsService
{
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<PaymentResponse> payments)> GetPayments(DateTime from, DateTime to);
    Task<(bool isSucceed, IActionResult? actionResult, string url)> GetPaymentUrl(IList<StrippedFlattenedEnrollment> input);
    Task<(bool isSucceed, IActionResult? actionResult)> ReceiveCallback(IFormCollection forms);
}