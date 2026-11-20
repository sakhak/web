using AssetManagement.Application.Features.Depreciation;
using AssetManagement.Web.Api;
using AssetManagement.Web.Api.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers;

[Authorize(AuthenticationSchemes = "JwtAuth")]
public class DepreciationProfileController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] bool activeOnly = true) =>
        FromResult(await Sender.Send(new GetDepreciationProfilesQuery(activeOnly)));

    [HttpPost]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "DEPRECIATION_MANAGE")]
    public async Task<IActionResult> Create([FromBody] DepreciationProfileRequest request) =>
        FromResult(await Sender.Send(new CreateDepreciationProfileCommand(
            request.Code, request.Name, request.UsefulLifeMonths, request.SalvagePercent, Actor)));

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "DEPRECIATION_MANAGE")]
    public async Task<IActionResult> Update(int id, [FromBody] DepreciationProfileRequest request) =>
        FromResult(await Sender.Send(new UpdateDepreciationProfileCommand(
            id, request.Name, request.UsefulLifeMonths, request.SalvagePercent, request.Active, Actor)));
}
