using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Application.Features.Maintenance;
using AssetManagement.Web.Api;
using AssetManagement.Web.Api.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Controllers;

[Authorize(AuthenticationSchemes = "JwtAuth")]
public class MaintenanceController : ApiControllerBase
{
    private const string Manage = "MAINT_MANAGE";
    private const string Complete = "MAINT_COMPLETE";

    /// <summary>
    /// Schedules that are overdue or inside their reminder window, most urgent first.
    /// With <paramref name="withinDays"/> it returns everything due within that many days instead.
    /// </summary>
    [HttpGet("due")]
    public async Task<IActionResult> GetDue([FromQuery] string? department, [FromQuery] string? location, [FromQuery] int? withinDays) =>
        FromResult(await Sender.Send(new GetMaintenanceDueQuery(department, location, withinDays)));

    /// <summary>
    /// Maintenance schedules. With <paramref name="updatedSince"/> the response also includes schedules that were
    /// deactivated after that time, so a client cache can drop them.
    /// </summary>
    [HttpGet("schedules")]
    public async Task<IActionResult> GetSchedules(
        [FromQuery] int? assetId, [FromQuery] bool activeOnly = false, [FromQuery] DateTime? updatedSince = null) =>
        FromResult(await Sender.Send(new GetMaintenanceSchedulesQuery(assetId, activeOnly, updatedSince)));

    [HttpGet("schedules/{id:int}")]
    public async Task<IActionResult> GetSchedule(int id) => FromResult(await Sender.Send(new GetMaintenanceScheduleByIdQuery(id)));

    [HttpPost("schedules")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = Manage)]
    [Idempotent(Required = true)]
    public async Task<IActionResult> CreateSchedule([FromBody] MaintenanceScheduleRequest request) =>
        FromResult(await Sender.Send(new CreateMaintenanceScheduleCommand(
            request.AssetId, request.Title, request.IntervalMonths, request.FirstDueDate, request.LeadDays, Actor)));

    [HttpPut("schedules/{id:int}")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = Manage)]
    public async Task<IActionResult> UpdateSchedule(int id, [FromBody] MaintenanceScheduleRequest request)
    {
        var current = await Sender.Send(new GetMaintenanceScheduleByIdQuery(id));
        if (!current.Succeeded) return FromResult(current);

        return FromResult(await Sender.Send(new UpdateMaintenanceScheduleCommand(
            id, request.Title, request.IntervalMonths, request.NextDueDate ?? current.Data.NextDueDate,
            request.LeadDays, request.Active, Actor)));
    }

    /// <summary>Work orders and history. Filter by asset, status, type, date range or <paramref name="updatedSince"/>.</summary>
    [HttpGet("records")]
    public async Task<IActionResult> GetRecords(
        [FromQuery] int? assetId, [FromQuery] string? status, [FromQuery] string? type,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? search, [FromQuery] DateTime? updatedSince,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20) =>
        Ok(await Sender.Send(new GetMaintenanceRecordsPagedQuery(
            new MaintenanceRecordFilter(assetId, status, type, from, to, search, updatedSince), pageNumber, pageSize)));

    /// <summary>The maintenance history of one asset (same as GET records?assetId=).</summary>
    [HttpGet("~/api/Asset/{assetId:int}/maintenance")]
    public async Task<IActionResult> GetAssetHistory(int assetId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20) =>
        Ok(await Sender.Send(new GetMaintenanceRecordsPagedQuery(new MaintenanceRecordFilter(AssetId: assetId), pageNumber, pageSize)));

    [HttpGet("records/{id:int}")]
    public async Task<IActionResult> GetRecord(int id) => FromResult(await Sender.Send(new GetMaintenanceRecordByIdQuery(id)));

    /// <summary>Creates a planned work order, for a schedule or ad hoc.</summary>
    [HttpPost("records")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = Manage)]
    [Idempotent(Required = true)]
    public async Task<IActionResult> Plan([FromBody] PlanMaintenanceRequest request) =>
        FromResult(await Sender.Send(new PlanMaintenanceCommand(
            request.AssetId, request.ScheduleId, request.Type, request.Title, request.PlannedDate, request.Notes,
            request.TakesAssetOutOfService, request.SupplierId, request.VendorName, Actor)));

    /// <summary>Starts a planned work order. When it takes the asset out of service, the asset status becomes Maintenance.</summary>
    [HttpPost("records/{id:int}/start")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = Complete)]
    [Idempotent(Required = true)]
    public async Task<IActionResult> Start(int id) => FromResult(await Sender.Send(new StartMaintenanceCommand(id, Actor)));

    /// <summary>Completes a planned or in-progress work order and moves the schedule to its next due date.</summary>
    [HttpPost("records/{id:int}/complete")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = Complete)]
    [Idempotent(Required = true)]
    public async Task<IActionResult> CompleteRecord(int id, [FromBody] CompleteMaintenanceRequest request) =>
        FromResult(await Sender.Send(new CompleteMaintenanceCommand(
            id, request.PerformedDate, request.PerformedBy, request.Cost, request.Currency,
            request.SupplierId, request.VendorName, request.Notes, Actor)));

    [HttpPost("records/{id:int}/cancel")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = Manage)]
    [Idempotent(Required = true)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelMaintenanceRequest request) =>
        FromResult(await Sender.Send(new CancelMaintenanceCommand(id, request.Reason, Actor)));

    /// <summary>
    /// Logs finished work in a single call (the call an app queues while offline). Give a schedule or an asset.
    /// If the schedule already has an open work order, that order is completed instead.
    /// </summary>
    [HttpPost("work")]
    [Authorize(AuthenticationSchemes = "JwtAuth", Policy = Complete)]
    [Idempotent(Required = true)]
    public async Task<IActionResult> RecordWork([FromBody] RecordMaintenanceWorkRequest request) =>
        FromResult(await Sender.Send(new RecordMaintenanceWorkCommand(
            request.AssetId, request.ScheduleId, request.Type, request.Title, request.PerformedDate, request.PerformedBy,
            request.Cost, request.Currency, request.SupplierId, request.VendorName, request.Notes, Actor)));
}
