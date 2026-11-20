using AssetManagement.Application.Contracts.Onboarding;
using AssetManagement.Application.Features.Onboarding;
using AssetManagement.Web.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Onboarding.GoodsReceipts;

[Authorize(Policy = "GRN_CREATE")]
public class CreateModel(ISender sender) : PageModel
{
    [BindProperty]
    public GoodsReceiptForm Form { get; set; } = new();

    public PurchaseOrderDto Order { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int poId)
    {
        if (!await LoadOrderAsync(poId)) return NotFound();

        if (Order.Status is not ("Approved" or "PartiallyReceived"))
        {
            TempData["Error"] = "Goods can only be received against an approved purchase order.";
            return RedirectToPage("/Onboarding/PurchaseOrders/Details", new { id = poId });
        }

        Form = new GoodsReceiptForm
        {
            PurchaseOrderId = poId,
            ReceivedBy = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name,
            Lines = Order.Lines.Select(l => new GoodsReceiptLineInput { PurchaseOrderLineId = l.Id }).ToList()
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int poId)
    {
        if (!await LoadOrderAsync(poId)) return NotFound();

        ModelState.Clear();
        var lines = Form.Lines.Where(l => l.QuantityReceived > 0).ToList();
        if (lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Enter a received quantity for at least one line.");
            return Page();
        }

        try
        {
            var result = await sender.Send(new CreateGoodsReceiptCommand(
                poId, Form.ReceivedDate, Form.ReceivedBy, Form.Remarks, lines,
                User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = $"Goods receipt {result.Data.GrnNumber} recorded. You can now tag the received units as assets.";
            return RedirectToPage("Details", new { id = result.Data.Id });
        }
        catch (ValidationException ex)
        {
            ModelState.AddFluentErrors(ex, nameof(Form));
            return Page();
        }
    }

    private async Task<bool> LoadOrderAsync(int poId)
    {
        var result = await sender.Send(new GetPurchaseOrderByIdQuery(poId));
        if (!result.Succeeded) return false;

        Order = result.Data;
        return true;
    }
}
