using AssetManagement.Application.Abstractions.Services;
using AssetManagement.Application.Common.Models;
using AssetManagement.Application.Contracts;
using AssetManagement.Application.Contracts.Responses;
using AssetManagement.Application.Features.Movements;
using AssetManagement.Web.Api;
using AssetManagement.Web.Api.Requests;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers;

[Authorize(AuthenticationSchemes = "JwtAuth")]
public class AssetMovementController(IMapper mapper, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? searchBy, [FromQuery] string? search, [FromQuery] string? filter,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await Sender.Send(new GetMovementsPagedQuery(searchBy, search, filter, pageNumber, pageSize));
        return Ok(result.Map(mapper.Map<AssetMovementResponse>));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await Sender.Send(new GetMovementDetailsQuery(id));
        return FromResult(result.Map(mapper.Map<MovementDetailsResponse>));
    }

    [HttpPost]
    [Idempotent]
    public async Task<IActionResult> Create([FromBody] CreateAssetMovementRequestDto request)
    {
        var requestedBy = currentUser.DisplayName ?? currentUser.Email ?? currentUser.StaffId ?? "Unknown";

        var command = new CreateMovementCommand
        {
            Movement = mapper.Map<MovementInput>(request),
            MovementType = request.MovementType,
            ApprovalRequired = request.ApprovalRequired,
            ApprovedBy = requestedBy,
            Repair = request.RepairRecord is null ? null : mapper.Map<RepairInput>(request.RepairRecord),
            Disposal = request.DisposalRecord is null ? null : mapper.Map<DisposalInput>(request.DisposalRecord),
            StaffId = currentUser.StaffId
        };

        var result = await Sender.Send(command);
        return FromResult(result.Map(mapper.Map<AssetMovementResponse>));
    }
}
