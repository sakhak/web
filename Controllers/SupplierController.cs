using AssetManagement.Application.Features.Suppliers;
using AssetManagement.Domain.Enums;
using AssetManagement.Web.Api;
using AssetManagement.Web.Api.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers;

[Authorize(AuthenticationSchemes = "JwtAuth")]
public class SupplierController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? search, [FromQuery] BaseStatus? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20) =>
        FromResult(await Sender.Send(new GetSuppliersPagedQuery(search, status, Math.Max(pageNumber, 1), Math.Clamp(pageSize, 1, 200))));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => FromResult(await Sender.Send(new GetSupplierByIdQuery(id)));

    [HttpPost]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "SUPPLIER_MANAGE")]
    public async Task<IActionResult> Create([FromBody] SupplierRequest request) =>
        FromResult(await Sender.Send(new CreateSupplierCommand(request.ToInput(), Actor)));

    [HttpPut("{id:guid}")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "SUPPLIER_MANAGE")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SupplierRequest request) =>
        FromResult(await Sender.Send(new UpdateSupplierCommand(id, request.ToInput(), request.Active, Actor)));

    [HttpPatch("{id:guid}/status")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = "SUPPLIER_MANAGE")]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] SupplierStatusRequest request) =>
        FromResult(await Sender.Send(new SetSupplierStatusCommand(id, request.Active, Actor)));
}
