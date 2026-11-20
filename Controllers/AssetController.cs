using AssetManagement.Application.Common.Models;
using AssetManagement.Application.Contracts.Responses;
using AssetManagement.Application.Features.Assets;
using AssetManagement.Application.Features.Categories;
using AssetManagement.Application.Features.Depreciation;
using AssetManagement.Domain.Entities;
using AssetManagement.Web.Api;
using AssetManagement.Web.Api.Requests;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers;

[Authorize(AuthenticationSchemes = "JwtAuth")]
public class AssetController(IMapper mapper) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? searchBy, [FromQuery] string? search,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] DateTime? updatedSince = null)
    {
        var result = await Sender.Send(new GetAssetsPagedQuery(searchBy, search, pageNumber, pageSize, updatedSince));
        return Ok(result.Map(mapper.Map<AssetResponse>));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await Sender.Send(new GetAssetByIdQuery(id));
        return FromResult(result.Map(mapper.Map<AssetResponse>));
    }

    [HttpPost]
    [Idempotent]
    public async Task<IActionResult> Create([FromForm] AssetCreateRequestDto request)
    {
        byte[]? imageBytes = null;
        if (request.AssetImage is { Length: > 0 })
        {
            using var ms = new MemoryStream();
            await request.AssetImage.CopyToAsync(ms);
            imageBytes = ms.ToArray();
        }

        var command = mapper.Map<CreateAssetCommand>(request) with
        {
            ImageData = imageBytes,
            ImageFileName = request.AssetImage?.FileName,
            ImageContentType = request.AssetImage?.ContentType
        };

        var result = await Sender.Send(command);
        return FromResult(result.Map(mapper.Map<AssetResponse>));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AssetUpdateRequestDto request)
    {
        var command = mapper.Map<UpdateAssetCommand>(request) with { Id = id };

        var result = await Sender.Send(command);
        if (!result.Succeeded) return FromResult(result);

        var updated = await Sender.Send(new GetAssetByIdQuery(id));
        return FromResult(updated.Map(mapper.Map<AssetResponse>));
    }

    [HttpGet("{id:int}/depreciation")]
    public async Task<IActionResult> GetDepreciation(int id, [FromQuery] DateTime? asOf) =>
        FromResult(await Sender.Send(new GetAssetDepreciationQuery(id, asOf)));

    [HttpPut("{id:int}/depreciation-profile")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "DEPRECIATION_MANAGE")]
    public async Task<IActionResult> AssignDepreciationProfile(int id, [FromBody] AssignDepreciationProfileRequest request) =>
        FromResult(await Sender.Send(new AssignDepreciationProfileCommand(id, request.ProfileId, request.StartDate)));

    [HttpPut("{id:int}/category")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "ASSET_EDIT")]
    public async Task<IActionResult> AssignCategory(int id, [FromBody] AssignAssetCategoryRequest request) =>
        FromResult(await Sender.Send(new AssignAssetCategoryCommand(id, request.CategoryId)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => FromResult(await Sender.Send(new DeleteAssetCommand(id)));
}
