using AssetManagement.Application.Features.Categories;
using AssetManagement.Web.Api;
using AssetManagement.Web.Api.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers;

[Authorize(AuthenticationSchemes = "JwtAuth")]
public class AssetCategoryController : ApiControllerBase
{
    /// <summary>
    /// Active categories. With <paramref name="updatedSince"/> the response also includes categories that were
    /// deactivated after that time, so a client cache can drop them.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] bool activeOnly = true, [FromQuery] DateTime? updatedSince = null) =>
        FromResult(await Sender.Send(new GetAssetCategoriesQuery(activeOnly, updatedSince)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => FromResult(await Sender.Send(new GetAssetCategoryByIdQuery(id)));

    [HttpPost]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "CATEGORY_MANAGE")]
    public async Task<IActionResult> Create([FromBody] AssetCategoryRequest request) =>
        FromResult(await Sender.Send(new CreateAssetCategoryCommand(
            request.Code, request.Name, request.Description, request.DefaultProfileId, Actor)));

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "CATEGORY_MANAGE")]
    public async Task<IActionResult> Update(int id, [FromBody] AssetCategoryRequest request) =>
        FromResult(await Sender.Send(new UpdateAssetCategoryCommand(
            id, request.Name, request.Description, request.DefaultProfileId, request.Active, Actor)));
}
