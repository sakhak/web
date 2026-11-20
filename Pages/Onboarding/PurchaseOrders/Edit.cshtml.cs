using AssetManagement.Application.Contracts.Onboarding;
using AssetManagement.Application.Features.Depreciation;
using AssetManagement.Application.Features.Onboarding;
using AssetManagement.Web.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Onboarding.PurchaseOrders;

[Authorize(Policy = "PO_CREATE")]
public class EditModel(ISender sender) : PageModel
{
    [BindProperty]
    public PurchaseOrderForm Form { get; set; } = new();

    public string PoNumber { get; set; } = string.Empty;
    public List<DepreciationProfileDto> Profiles { get; set; } = [];
    public List<AssetManagement.Application.Contracts.AssetCategoryDto> Categories { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await sender.Send(new GetPurchaseOrderByIdQuery(id));
        if (!result.Succeeded) return NotFound();

        if (result.Data.Status != "Draft")
        {
            TempData["Error"] = "Only a draft purchase order can be edited.";
            return RedirectToPage("Details", new { id });
        }

        var order = result.Data;
        PoNumber = order.PoNumber;
        Form = new PurchaseOrderForm
        {
            Id = order.Id,
            VendorName = order.VendorName,
            OrderDate = order.OrderDate,
            Currency = order.Currency,
            Remarks = order.Remarks,
            Lines = order.Lines.Select(l => new PurchaseOrderLineInput
            {
                Description = l.Description,
                CategoryCode = l.CategoryCode,
                DepreciationProfileId = l.DepreciationProfileId,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost
            }).ToList()
        };

        await LoadProfilesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await LoadProfilesAsync();
        PoNumber = (await sender.Send(new GetPurchaseOrderByIdQuery(id))).Data?.PoNumber ?? string.Empty;
        if (!ModelState.IsValid) return Page();

        try
        {
            var result = await sender.Send(new UpdatePurchaseOrderCommand(
                id, Form.VendorName, null, Form.OrderDate, Form.Currency, Form.Remarks, Form.Lines,
                User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = "Purchase order updated.";
            return RedirectToPage("Details", new { id });
        }
        catch (ValidationException ex)
        {
            ModelState.AddFluentErrors(ex, nameof(Form));
            return Page();
        }
    }

    private async Task LoadProfilesAsync()
    {
        Profiles = (await sender.Send(new GetDepreciationProfilesQuery())).Data ?? [];
        Categories = (await sender.Send(new AssetManagement.Application.Features.Categories.GetAssetCategoriesQuery())).Data ?? [];
    }
}
