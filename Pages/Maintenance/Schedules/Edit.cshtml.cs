using AssetManagement.Application.Features.Maintenance;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Enums;
using AssetManagement.Web.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Maintenance.Schedules;

[Authorize(Policy = "MAINT_MANAGE")]
public class EditModel(ISender sender) : PageModel
{
    [BindProperty]
    public MaintenanceScheduleForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await sender.Send(new GetMaintenanceScheduleByIdQuery(id));
        if (!result.Succeeded) return NotFound();

        var s = result.Data;
        Form = new MaintenanceScheduleForm
        {
            Id = s.Id,
            AssetNumber = s.AssetNumber,
            Title = s.Title,
            IntervalMonths = s.IntervalMonths,
            NextDueDate = s.NextDueDate,
            LeadDays = s.LeadDays,
            Active = s.Status == AssetManagement.Domain.Constants.StatusCode.Active
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid) return Page();

        if (Form.NextDueDate is null)
        {
            ModelState.AddModelError("Form.NextDueDate", "The next due date is required.");
            return Page();
        }

        try
        {
            var result = await sender.Send(new UpdateMaintenanceScheduleCommand(
                id, Form.Title, Form.IntervalMonths, Form.NextDueDate.Value, Form.LeadDays, Form.Active, User.ActorName()));

            if (result.Status == HttpStatus.NotFound) return NotFound();
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = $"Schedule '{result.Data.Title}' updated.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            ModelState.AddFluentErrors(ex, nameof(Form));
            return Page();
        }
    }
}
