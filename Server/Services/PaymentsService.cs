using System.Text;
using System.Text.Json;
using LiqPayIntegration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Constants;
using Server.Data;
using Server.Models;
using SharedModels.Responses;

namespace Server.Services;

public class PaymentsService : IPaymentsService
{
    readonly ApplicationDbContext _dbContext;

    readonly string _userId;

    // LiqpayIntegration. Change 3-rd argument` address to current machine address in its network. Don't forget about port.
    LiqPay liqPay = new LiqPay("sandbox_i23432845039", "sandbox_gymL9PdryqdfAznNQbb7ynLvASDQ5SJCCNJvF2iV", "http://localhost:5006/api/payment/callback/");

    public PaymentsService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _userId = httpContextAccessor.HttpContext.User.Claims
            .FirstOrDefault(c => c.Properties.Values.Any(v => v == JwtStandardClaimNames.Sub))?.Value;
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, IEnumerable<PaymentResponse> payments)> GetPayments(DateTime from, DateTime to)
    {
        return(true, null, (await liqPay.PaymentArchive(from, to)).AsEnumerable());
    }

    public async Task<(bool isSucceed, IActionResult? actionResult, string url)> GetPaymentUrl(IList<StrippedFlattenedEnrollment> input)
    {
        if (input.Count < 1)
            return (false, new BadRequestResult(), "");

        input.ToList().OrderBy(e => e.Order);

        List<Ticket> tickets = new List<Ticket>();
        foreach (StrippedFlattenedEnrollment e in input)
        {
            tickets.Add(new Ticket
            {
                VehicleEnrollmentId = e.Id,
                FirstRouteAddressId = e.DepartureAddressId,
                LastRouteAddressId = e.ArrivalAddressId
            });
        }

        TicketGroup ticketGroup = new TicketGroup
        {
            Tickets = tickets,
            UserId = _userId
        };

        _dbContext.TicketGroups.Add(ticketGroup);
        await _dbContext.SaveChangesAsync();

        int orderId = ticketGroup.Id;

        var ticketGroups = _dbContext.TicketGroups;
        var dbTicketGroup = await _dbContext.TicketGroups
                 .Include(tg => tg.Tickets)
                 .ThenInclude(t => t.VehicleEnrollment)
                 .ThenInclude(ve => ve.RouteAddressDetails)
                 .ThenInclude(ve => ve.RouteAddress)
                 .ThenInclude(ra => ra.Route)
                 .ThenInclude(r => r.RouteAddresses)
                 .ThenInclude(ra => ra.Address)
                 .ThenInclude(a => a.City)
                 .ThenInclude(c => c.State)
                 .ThenInclude(s => s.Country)
                 .FirstAsync(tg => tg.Id == orderId);

        double cost = dbTicketGroup.GetCost();
        string description = 
            $"Поїздка з { dbTicketGroup.GetDepartureAddress().GetFullName()}\n"+
            $"До { dbTicketGroup.GetArrivalAddress().GetFullName()}";
        string text = await liqPay.GetPaymentUrl(cost, description, orderId.ToString());

        return (true, new OkResult(), text);
    }

    public async Task<(bool isSucceed, IActionResult? actionResult)> ReceiveCallback(IFormCollection forms)
    {
        if (!forms.Keys.Contains("data"))
            return (false, new BadRequestResult());

        var baseData = forms["data"].First();
        
        var data = Encoding.UTF8.GetString(Convert.FromBase64String(baseData));
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(data);

        if (!jsonData.ContainsKey("status") || !jsonData.ContainsKey("order_id"))
            return (false, new BadRequestResult());

        if (jsonData["status"].GetString() != "success")
            return (true, new OkResult());

        var orderId = int.Parse(jsonData["order_id"].GetString());
        var ticket = _dbContext.TicketGroups
            .Find(orderId);
        ticket.PurchaseDateTimeUtc = DateTime.UtcNow;

        _dbContext.Update(ticket);
        await _dbContext.SaveChangesAsync();
        
        return (true, new OkResult());
    }

}