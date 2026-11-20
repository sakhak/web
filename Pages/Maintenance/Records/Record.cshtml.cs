using AssetManagement.Application.Features.Assets;
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

[Authorize(Policy = "MAINT_COMPLETE")]
public class RecordModel(ISender sender) : PageModel
{
    [BindProperty]
    public RecordWorkForm Form { get; set; } = new();

    public List<AssetManagement.Domain.Entities.Supplier> Suppliers { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync([FromQuery] int? scheduleId, [FromQuery] string? assetNumber)
    {
        Form.AssetNumber = assetNumber;

        if (scheduleId is { } id)
        {
            var schedule = await sender.Send(new GetMaintenanceScheduleByIdQuery(id));
            if (!schedule.Succeeded) return NotFound();

            Form.ScheduleId = id;
            Form.AssetNumber = schedule.Data.AssetNumber;
            Form.Title = schedule.Data.Title;
            Form.Type = MaintenanceType.Preventive;
        }

        await LoadSuppliersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadSuppliersAsync();
        if (!ModelState.IsValid) return Page();

        if (Form.PerformedDate is null)
        {
            ModelState.AddModelError("Form.PerformedDate", "The work date is required.");
            return Page();
        }

        var asset = await sender.Send(new GetAssetByNumberQuery(Form.AssetNumber ?? string.Empty));
        if (!asset.Succeeded)
        {
            ModelState.AddModelError("Form.AssetNumber", asset.Message);
            return Page();
        }

        try
        {
            var result = await sender.Send(new RecordMaintenanceWorkCommand(
                asset.Data.Id, Form.ScheduleId, Form.Type, Form.Title, Form.PerformedDate.Value, Form.PerformedBy, Form.Cost,
                Form.Currency, Form.SupplierId, Form.VendorName, Form.Notes, User.ActorName()));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = "Maintenance recorded.";
            return RedirectToPage("Details", new { id = result.Data.Id });
        }
        catch (ValidationException ex)
        {
            ModelState.AddFluentErrors(ex, nameof(Form));
            return Page();
        }
    }

    private async Task LoadSuppliersAsync() => Suppliers = await sender.Send(new GetActiveSuppliersQuery());
}
