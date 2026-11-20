using AssetManagement.Application.Abstractions.Services;
using AssetManagement.Application.Common;
using static AssetManagement.Application.Common.Constants.ApplicationConstants;

namespace AssetManagement.Web.Services;

public sealed class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private System.Security.Claims.ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    public string? Email => Principal?.Identity?.Name;
    public string? StaffId => Principal?.FindFirst(ClaimType.StaffId)?.Value;
    public string? DisplayName => Principal?.FindFirst(ClaimType.DisplayName)?.Value;
    public string? BranchCode => Principal?.GetBranchCode();
}
