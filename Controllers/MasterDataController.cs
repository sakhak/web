using AssetManagement.Application.Features.Settings;
using AssetManagement.Domain.Constants;
using AssetManagement.Web.Api;
using AssetManagement.Web.Api.Requests;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers;

[Authorize(AuthenticationSchemes = "JwtAuth")]
public class MasterDataController(IMapper mapper) : ApiControllerBase
{
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        var result = await Sender.Send(new GetSettingsByTypeCodeQuery(SettingTypeCode.Department));
        return FromResult(result.Map(list => list.Select(mapper.Map<SettingResponse>).ToList()));
    }
}
