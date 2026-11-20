using AssetManagement.Application.Common.Constants;
using AssetManagement.Application.Features.Movements;
using AssetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.AssetMovements;

public class RepairUpdateModel(ISender sender) : PageModel
{
    [BindProperty]
    public RepairRecord? RepairRecord { get; set; }
    [BindProperty]
    public string? StaffId { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        StaffId = User.Claims.FirstOrDefault(x => x.Type == "StaffId")?.Value;
        if (id == null) return NotFound();

        var result = await sender.Send(new GetRepairRecordByIdQuery(id.Value));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded) return NotFound();

        RepairRecord = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        if (RepairRecord == null) return RedirectToPage("/AssetMovements/Index");

        var result = await sender.Send(new UpdateRepairStatusCommand(
            RepairRecord.Id, RepairRecord.AssetId, RepairRecord.RepairStatus, RepairRecord.Notes, StaffId));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        return RedirectToPage("/AssetMovements/Details", new { id = RepairRecord.AssetId });
    }
}
