using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProjectManagement.Api.Controllers;

[ApiController]
[Authorize]
public abstract class BaseController : ControllerBase
{
  protected string? GetCurrentUserId()
  {
    return User.FindFirstValue(ClaimTypes.NameIdentifier);
  }
}