using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Responses;

namespace Server.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/secured")]
[ApiController]
public class SecuredController : ControllerBase
{
     [HttpGet]
     public async Task<IActionResult> GetSecuredData()
     {
          return Ok(new ResponseBase
          {
               Message = "This Secured Data is available only for Authenticated Users with Admin role."
          });
     }
}