using AssetManagement.Application.Abstractions.Services;
using AssetManagement.Application.Common.Models;
using AssetManagement.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Api;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _sender;
    private ICurrentUser? _currentUser;

    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ICurrentUser CurrentUser => _currentUser ??= HttpContext.RequestServices.GetRequiredService<ICurrentUser>();

    protected string? Actor => CurrentUser.DisplayName ?? CurrentUser.Email ?? CurrentUser.StaffId;

    protected IActionResult FromResult(Result result)
    {
        if (result.Succeeded) return Ok(result);

        var status = result.Status == 0 ? HttpStatus.BadRequest : result.Status;
        return StatusCode((int)status, result);
    }
}
