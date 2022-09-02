using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/secured")]
[ApiController]
public class SecuredController : ControllerBase
{
     [HttpGet]
     public async Task<IActionResult> GetSecuredData()
     {
          return Ok("This Secured Data is available only for Authenticated Users with Admin role.");
     }
}