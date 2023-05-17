using LiqPayIntegration;
using Microsoft.AspNetCore.Mvc;
using SharedModels.DataTransferObjects;
using System.Dynamic;

namespace Server.Services;

public interface IPaymentsService
{
    Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<PaymentResponse> payments)> GetPayments(DateTime from, DateTime to);
    Task<(bool isSucceed, IActionResult? actionResult, string url)> GetPaymentUrl(PaymentDto payment);
}