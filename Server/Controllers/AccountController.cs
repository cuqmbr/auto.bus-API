using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services;
using SharedModels.Requests;
using SharedModels.Requests.Account;

namespace Server.Controllers;
 
[Authorize]
[Route("api/account")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountManagementService _accountManagementService;

    public AccountController(IAccountManagementService accountManagementService)
    {
        _accountManagementService = accountManagementService;
    }

    [HttpPost("changeInformation")]
    public async Task<IActionResult> ChangeInformation([FromBody] ChangeInformationRequest request)
    {
        var result = await _accountManagementService.ChangeInformation(request);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok();
    }

    [HttpPost("changeEmail")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
    {
        var result = await _accountManagementService.ChangeEmail(request);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok();
    }
    
    [HttpPost("confirmChangeEmail")]
    public async Task<IActionResult> ConfirmChangeEmail([FromBody] ConfirmChangeEmailRequest request)
    {
        var result = await _accountManagementService.ConfirmChangeEmail(request);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok();
    }
    
    [HttpPost("changePhoneNumber")]
    public async Task<IActionResult> ChangePhoneNumber([FromBody] ChangePhoneNumberRequest request)
    {
        var result = await _accountManagementService.ChangePhoneNumber(request);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok();
    }

    [HttpPost("confirmChangePhoneNumber")]
    public async Task<IActionResult> ConfirmPhoneNumber([FromBody] ConfirmChangePhoneNumberRequest request)
    {
        var result = await _accountManagementService.ConfirmPhoneNumberChange(request);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok();
    }
    
    [HttpPost("changePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _accountManagementService.ChangePassword(request);
        
        if (!result.isSucceed)
        {
            return result.actionResult;
        }

        return Ok();
    }
}