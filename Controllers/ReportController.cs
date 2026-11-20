using AssetManagement.Application.Features.Depreciation;
using AssetManagement.Web.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers;

[Authorize(AuthenticationSchemes = "JwtAuth", Policy = "REPORT_DEPRECIATION")]
public class ReportController : ApiControllerBase
{
    [HttpGet("depreciation")]
    public async Task<IActionResult> Depreciation(
        [FromQuery] string? department, [FromQuery] string? location,
        [FromQuery] int? profileId, [FromQuery] DateTime? asOf) =>
        FromResult(await Sender.Send(new GetDepreciationSummaryQuery(department, location, profileId, asOf)));
}
