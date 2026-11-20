using AssetManagement.Application.Common;
using AssetManagement.Application.Contracts;
using AssetManagement.Application.Features.Maintenance;
using AssetManagement.Application.Features.Suppliers;
using AssetManagement.Domain.Constants;
using AssetManagement.Web.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Maintenance.Records;

[Authorize]
public class DetailsModel(ISender sender) : PageModel
{
    public MaintenanceRecordDto Record { get; private set; } = null!;
    public List<AssetManagement.Domain.Entities.Supplier> Suppliers { get; private set; } = [];

    [BindProperty]
    public CompleteWorkForm Complete { get; set; } = new();

    [BindProperty]
    public string? CancelReason { get; set; }

    public bool CanManage => User.GetUserPermAccess().Contains("MAINT_MANAGE");
    public bool CanComplete => User.GetUserPermAccess().Contains("MAINT_COMPLETE");
    public bool IsOpen => Record.Status is MaintenanceStatus.Planned or MaintenanceStatus.InProgress;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await LoadAsync(id)) return NotFound();

        Complete.VendorName = Record.VendorName;
        Complete.SupplierId = Record.SupplierId;
        return Page();
    }

    public async Task<IActionResult> OnPostStartAsync(int id)
    {
        if (!CanComplete) return Forbid();

        var result = await sender.Send(new StartMaintenanceCommand(id, User.ActorName()));
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Work started." : result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCompleteAsync(int id)
    {
        if (!CanComplete) return Forbid();
        if (!await LoadAsync(id)) return NotFound();

        if (Complete.PerformedDate is null)
            ModelState.AddModelError("Complete.PerformedDate", "The work date is required.");
        if (!ModelState.IsValid) return Page();

        try
        {
            var result = await sender.Send(new CompleteMaintenanceCommand(
                id, Complete.PerformedDate!.Value, Complete.PerformedBy, Complete.Cost, Complete.Currency,
                Complete.SupplierId, Complete.VendorName, Complete.Notes, User.ActorName()));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = "Work completed.";
            return RedirectToPage(new { id });
        }
        catch (ValidationException ex)
        {
            ModelState.AddFluentErrors(ex, nameof(Complete));
            return Page();
        }
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        if (!CanManage) return Forbid();

        var result = await sender.Send(new CancelMaintenanceCommand(id, CancelReason, User.ActorName()));
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Work order cancelled." : result.Message;
        return RedirectToPage(new { id });
    }

    private async Task<bool> LoadAsync(int id)
    {
        var result = await sender.Send(new GetMaintenanceRecordByIdQuery(id));
        if (!result.Succeeded) return false;

        Record = result.Data;
        Suppliers = await sender.Send(new GetActiveSuppliersQuery());
        return true;
    }
}
