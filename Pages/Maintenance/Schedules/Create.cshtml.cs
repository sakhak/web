using AssetManagement.Application.Features.Assets;
using AssetManagement.Application.Features.Maintenance;
using AssetManagement.Web.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Maintenance.Schedules;

[Authorize(Policy = "MAINT_MANAGE")]
public class CreateModel(ISender sender) : PageModel
{
    [BindProperty]
    public MaintenanceScheduleForm Form { get; set; } = new();

    public void OnGet([FromQuery] string? assetNumber) => Form.AssetNumber = assetNumber ?? string.Empty;

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var asset = await sender.Send(new GetAssetByNumberQuery(Form.AssetNumber));
        if (!asset.Succeeded)
        {
            ModelState.AddModelError("Form.AssetNumber", asset.Message);
            return Page();
        }

        try
        {
            var result = await sender.Send(new CreateMaintenanceScheduleCommand(
                asset.Data.Id, Form.Title, Form.IntervalMonths, Form.NextDueDate, Form.LeadDays, User.ActorName()));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = $"Schedule '{result.Data.Title}' created for {result.Data.AssetNumber}.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            ModelState.AddFluentErrors(ex, nameof(Form));
            return Page();
        }
    }
}
