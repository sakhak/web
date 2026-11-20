using AssetManagement.Application.Features.Onboarding;
using AssetManagement.Web.Api;
using AssetManagement.Web.Api.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers;

[Authorize(AuthenticationSchemes = "JwtAuth")]
public class PurchaseOrderController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? search, [FromQuery] string? status,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10) =>
        Ok(await Sender.Send(new GetPurchaseOrdersPagedQuery(search, status, pageNumber, pageSize)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => FromResult(await Sender.Send(new GetPurchaseOrderByIdQuery(id)));

    [HttpPost]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "PO_CREATE")]
    public async Task<IActionResult> Create([FromBody] PurchaseOrderRequest request) =>
        FromResult(await Sender.Send(new CreatePurchaseOrderCommand(
            request.VendorName, request.SupplierId, request.OrderDate, request.Currency, request.Remarks, request.Lines, Actor)));

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "PO_CREATE")]
    public async Task<IActionResult> Update(int id, [FromBody] PurchaseOrderRequest request) =>
        FromResult(await Sender.Send(new UpdatePurchaseOrderCommand(
            id, request.VendorName, request.SupplierId, request.OrderDate, request.Currency, request.Remarks, request.Lines, Actor)));

    [HttpPost("{id:int}/approve")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "PO_APPROVE")]
    public async Task<IActionResult> Approve(int id) => FromResult(await Sender.Send(new ApprovePurchaseOrderCommand(id, Actor)));

    [HttpPost("{id:int}/cancel")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "PO_CREATE")]
    public async Task<IActionResult> Cancel(int id) => FromResult(await Sender.Send(new CancelPurchaseOrderCommand(id, Actor)));
}
