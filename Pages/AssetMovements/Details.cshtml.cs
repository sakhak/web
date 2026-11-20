using AssetManagement.Application.Common.Constants;
using AssetManagement.Application.Features.Movements;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.AssetMovements;

public class DetailsModel(ISender sender) : PageModel
{
    public AssetMovement? AssetMovement { get; set; }
    public Asset? Asset { get; set; }
    public RepairRecord? RepairRecord { get; set; }
    public DisposalRecord? DisposalRecord { get; set; }
    public bool CanDispatch { get; set; }
    public bool NeedsApproval { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var result = await sender.Send(new GetMovementDetailsQuery(id.Value));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded) return NotFound();

        AssetMovement = result.Data.AssetMovement;
        Asset = AssetMovement.Asset;
        RepairRecord = result.Data.RepairRecord;
        DisposalRecord = result.Data.DisposalRecord;

        CanDispatch = AssetMovement.Status == MovementStatus.Pending ||
                      AssetMovement.Status == MovementStatus.PendingApproval && AssetMovement.ApprovalDate != null;

        NeedsApproval = AssetMovement.AwaitingApproval;
        return Page();
    }
}
