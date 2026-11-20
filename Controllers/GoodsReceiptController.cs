using AssetManagement.Application.Features.Onboarding;
using AssetManagement.Web.Api;
using AssetManagement.Web.Api.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers;

[Authorize(AuthenticationSchemes = "JwtAuth")]
public class GoodsReceiptController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? search, [FromQuery] int? purchaseOrderId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10) =>
        Ok(await Sender.Send(new GetGoodsReceiptsPagedQuery(search, purchaseOrderId, pageNumber, pageSize)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => FromResult(await Sender.Send(new GetGoodsReceiptByIdQuery(id)));

    [HttpPost]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "GRN_CREATE")]
    public async Task<IActionResult> Create([FromBody] GoodsReceiptRequest request) =>
        FromResult(await Sender.Send(new CreateGoodsReceiptCommand(
            request.PurchaseOrderId, request.ReceivedDate, request.ReceivedBy ?? Actor, request.Remarks, request.Lines, Actor)));

    [HttpPost("lines/{lineId:int}/assets")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "ASSET_TAG")]
    public async Task<IActionResult> TagAssets(int lineId, [FromBody] TagAssetsRequest request) =>
        FromResult(await Sender.Send(new TagAssetsFromGoodsReceiptCommand(
            lineId, request.Assets, request.DepreciationProfileId, request.DepreciationStartDate)));
}
