using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using SharedModels.Requests;
using SharedModels.Requests.Account;

namespace Server.Services;

public class AccountManagementService : IAccountManagementService
{
    private readonly IEmailSenderService _emailSender;
    private readonly UserManager<User> _userManager;
    private readonly ISessionUserService _sessionUserService;

    public AccountManagementService(IEmailSenderService emailSender, UserManager<User> userManager,
        ISessionUserService sessionUserService)
    {
        _emailSender = emailSender;
        _userManager = userManager;
        _sessionUserService = sessionUserService;
        
        _userManager.UserValidators.Clear();
    }

    public async Task<(bool isSucceed, IActionResult actionResult)> ChangeInformation(ChangeInformationRequest request)
    {
        var dbUser = await _userManager.FindByIdAsync(_sessionUserService.GetAuthUserId());

        dbUser.FirstName = request.FistName;
        dbUser.LastName = request.LastName;
        dbUser.Patronymic = request.Patronymic;
        dbUser.BirthDate = new DateTime(request.BirthDate.Year, request.BirthDate.Month, request.BirthDate.Day, 0, 0, 0, DateTimeKind.Utc);
        dbUser.Gender = request.Gender;

        await _userManager.UpdateAsync(dbUser);
        
        return (true, null!);
    }

    public async Task<(bool isSucceed, IActionResult actionResult)> ChangeEmail(ChangeEmailRequest request)
    {
        var dbUser = await _userManager.FindByIdAsync(_sessionUserService.GetAuthUserId());

        if (dbUser.Email.ToLower() == request.NewEmail.ToLower())
        {
            return (false, new BadRequestObjectResult("You must specify a new email"));
        }
        
        var changeEmailToken = await _userManager.GenerateChangeEmailTokenAsync(dbUser, request.NewEmail);
       
        var securityMessage =
            $"Someone is trying to change email address of your account to {request.NewEmail}. " +
            "If it is not you please follow account recovery procedure.";
        var confirmationMessage =
            "Someone changed account email to your address.\n" +
            $"Here is your confirmation code: {changeEmailToken}\n\n" +
            "If this was not you, please ignore this message.";

        try { await _emailSender.SendMail(dbUser.Email, "Security alert", securityMessage); }
        catch (Exception) {  /* ignored */ }
        
        try { await _emailSender.SendMail(request.NewEmail, "Change email confirmation", confirmationMessage); }
        catch (Exception) {  /* ignored */ }

        return (true, null!);
    }

    public async Task<(bool isSucceed, IActionResult actionResult)> ConfirmChangeEmail(ConfirmChangeEmailRequest request)
    {
        var dbUser = await _userManager.FindByIdAsync(_sessionUserService.GetAuthUserId());

        var result = await _userManager.ChangeEmailAsync(dbUser, request.NewEmail, request.Token);
        if (!result.Succeeded)
        {
            return (false, new BadRequestObjectResult($"Error confirming email change {request.NewEmail}"));
        }

        return (true, null!);
    }

    public async Task<(bool isSucceed, IActionResult actionResult)> ChangePhoneNumber(ChangePhoneNumberRequest request)
    {
        var dbUser = await _userManager.FindByIdAsync(_sessionUserService.GetAuthUserId());

        var changePhoneNumberToken = await _userManager.GenerateChangePhoneNumberTokenAsync(dbUser, request.PhoneNumber);
        
        var securityMessage =
            $"Someone is trying to change phone number of your account to {request.PhoneNumber}. " +
            "If it is not you please follow account recovery procedure.";

        try { await _emailSender.SendMail(dbUser.Email, "Security alert", securityMessage); }
        catch (Exception) {  /* ignored */ }
        
        // TODO: Send sms message to new phone number

        return (true, null!);
    }

    public async Task<(bool isSucceed, IActionResult actionResult)> ConfirmPhoneNumberChange(ConfirmChangePhoneNumberRequest request)
    {
        var dbUser = await _userManager.FindByIdAsync(_sessionUserService.GetAuthUserId());
        
        var result = await _userManager.ChangePhoneNumberAsync(dbUser, request.PhoneNumber, request.Token);
        if (!result.Succeeded)
        {
            return (false, new BadRequestObjectResult(result.Errors));
        }

        return (true, null!);
    }

    public async Task<(bool isSucceed, IActionResult actionResult)> ChangePassword(ChangePasswordRequest request)
    {
        var dbUser = await _userManager.FindByIdAsync(_sessionUserService.GetAuthUserId());

        if (!await _userManager.CheckPasswordAsync(dbUser, request.CurrentPassword))
        {
            return (false, new BadRequestObjectResult("Invalid current password"));
        }

        await _userManager.ChangePasswordAsync(dbUser, request.CurrentPassword, request.NewPassword);
        
        var securityMessage = "Someone is changed your account password." +
            "If this was not you please follow account recovery procedure.";

        try { await _emailSender.SendMail(dbUser.Email, "Security alert", securityMessage); }
        catch (Exception) {  /* ignored */ }
        
        return (true, null!);
    }
}