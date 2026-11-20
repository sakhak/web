using AssetManagement.Application.Common;
using AssetManagement.Application.Common.Models;
using AssetManagement.Application.Contracts;
using AssetManagement.Application.Features.Auth;
using AssetManagement.Web.Api;
using AssetManagement.Web.Api.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers;

public class AuthController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request) =>
        FromResult(await Sender.Send(new LoginWithAzureTokenCommand(request.Token)));

    /// <summary>
    /// The signed-in user with their current roles and permission keys. Call it to refresh permissions
    /// without logging in again, so the app can hide actions the user may not perform.
    /// </summary>
    [Authorize(AuthenticationSchemes = "JwtAuth")]
    [HttpGet("me")]
    public IActionResult Me()
    {
        string? Claim(string type) => User.FindFirst(type)?.Value;

        return Ok(Result<CurrentUserDto>.Success(new CurrentUserDto
        {
            Email = User.Identity?.Name,
            DisplayName = Claim("DisplayName"),
            StaffId = User.GetStaffId(),
            Department = Claim("Department"),
            Position = Claim("Position"),
            BranchName = Claim("BranchName"),
            BranchCode = Claim("BranchCode"),
            BranchShortName = Claim("BranchShortName"),
            EmpCategory = Claim("EmpCategory"),
            Proxy = Claim("Proxy"),
            Roles = User.GetRoles().ToList(),
            Permissions = User.GetUserPermAccess().Distinct().Order().ToList()
        }));
    }
}
