using Microsoft.AspNetCore.Mvc;
using SharedModels.Requests;
using SharedModels.Requests.Account;

namespace Server.Services;

public interface IAccountManagementService
{
    Task<(bool isSucceed, IActionResult actionResult)> ChangeInformation(ChangeInformationRequest request);

    Task<(bool isSucceed, IActionResult actionResult)> ChangeEmail(ChangeEmailRequest request);
    
    Task<(bool isSucceed, IActionResult actionResult)> ConfirmChangeEmail(ConfirmChangeEmailRequest request);

    Task<(bool isSucceed, IActionResult actionResult)> ChangePhoneNumber(ChangePhoneNumberRequest request);
    
    Task<(bool isSucceed, IActionResult actionResult)> ConfirmPhoneNumberChange(ConfirmChangePhoneNumberRequest request);

    Task<(bool isSucceed, IActionResult actionResult)> ChangePassword(ChangePasswordRequest request);
}