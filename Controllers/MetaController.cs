using System.Reflection;
using AssetManagement.Application.Common.Models;
using AssetManagement.Web.Api;
using AssetManagement.Web.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AssetManagement.Web.Controllers;

public class MetaController(IOptions<MobileOptions> options) : ApiControllerBase
{
    public const string ApiVersion = "1";
    public const string AppVersionHeader = "X-App-Version";

    /// <summary>
    /// Server and API version, the oldest supported app version and the server clock. The app calls this on start,
    /// before login, and sends its own version in the <c>X-App-Version</c> header to learn whether it must update.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("version")]
    public IActionResult GetVersion([FromHeader(Name = AppVersionHeader)] string? appVersion)
    {
        var mobile = options.Value;
        var serverVersion = typeof(MetaController).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?.Split('+')[0] ?? "1.0.0";

        var dto = new ApiVersionDto
        {
            ApiVersion = ApiVersion,
            ServerVersion = serverVersion,
            MinimumAppVersion = mobile.MinimumAppVersion,
            LatestAppVersion = mobile.LatestAppVersion,
            UpdateUrl = mobile.UpdateUrl,
            ServerTime = DateTime.Now,
            ServerTimeUtc = DateTime.UtcNow
        };

        if (AppVersion.TryParse(appVersion, out _))
        {
            dto.UpdateRequired = AppVersion.IsOlder(appVersion, mobile.MinimumAppVersion);
            dto.UpdateAvailable = !string.IsNullOrWhiteSpace(mobile.LatestAppVersion) && AppVersion.IsOlder(appVersion, mobile.LatestAppVersion);
        }

        return Ok(Result<ApiVersionDto>.Success(dto));
    }
}

public class ApiVersionDto
{
    public string ApiVersion { get; set; } = string.Empty;
    public string ServerVersion { get; set; } = string.Empty;
    public string MinimumAppVersion { get; set; } = string.Empty;
    public string? LatestAppVersion { get; set; }
    public string? UpdateUrl { get; set; }

    /// <summary>Only set when the request carried a parsable <c>X-App-Version</c>.</summary>
    public bool? UpdateRequired { get; set; }

    /// <summary>Only set when the request carried a parsable <c>X-App-Version</c>.</summary>
    public bool? UpdateAvailable { get; set; }

    /// <summary>Server local time; the app can compare it with its clock before sending scan times.</summary>
    public DateTime ServerTime { get; set; }
    public DateTime ServerTimeUtc { get; set; }
}
