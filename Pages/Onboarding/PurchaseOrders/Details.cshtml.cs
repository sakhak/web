using AssetManagement.Application.Common;
using AssetManagement.Application.Contracts.Onboarding;
using AssetManagement.Application.Features.Onboarding;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Onboarding.PurchaseOrders;

[Authorize]
public class DetailsModel(ISender sender) : PageModel
{
    public PurchaseOrderDto Order { get; set; } = new();
    public List<GoodsReceiptDto> Receipts { get; set; } = [];

    public bool CanCreate => HasPermission("PO_CREATE");
    public bool CanApprove => HasPermission("PO_APPROVE");
    public bool CanReceive => HasPermission("GRN_CREATE");

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await sender.Send(new GetPurchaseOrderByIdQuery(id));
        if (!result.Succeeded) return NotFound();

        Order = result.Data;
        Receipts = (await sender.Send(new GetGoodsReceiptsPagedQuery(null, id, 1, 50))).Data ?? [];
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        if (!HasPermission("PO_APPROVE")) return Forbid();

        var result = await sender.Send(new ApprovePurchaseOrderCommand(id, Actor));
        Flash(result.Succeeded, result.Message);
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        if (!HasPermission("PO_CREATE")) return Forbid();

        var result = await sender.Send(new CancelPurchaseOrderCommand(id, Actor));
        Flash(result.Succeeded, result.Message);
        return RedirectToPage(new { id });
    }

    private string? Actor => User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name;

    private bool HasPermission(string key) => User.GetUserPermAccess().Contains(key);

    private void Flash(bool success, string message) => TempData[success ? "Success" : "Error"] = message;
}
