using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Application.Common;

namespace SmartStock.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    protected bool IsAdmin => User.IsInRole("Admin");

    protected IActionResult FromResult<T>(ResultModel<T> result)
    {
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
