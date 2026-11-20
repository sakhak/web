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
public class CreateModel(ISender sender) : PageModel
{
    [BindProperty]
    public PurchaseOrderForm Form { get; set; } = new();

    public List<DepreciationProfileDto> Profiles { get; set; } = [];
    public List<AssetManagement.Application.Contracts.AssetCategoryDto> Categories { get; set; } = [];

    public async Task OnGetAsync() => await LoadProfilesAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadProfilesAsync();
        if (!ModelState.IsValid) return Page();

        try
        {
            var result = await sender.Send(new CreatePurchaseOrderCommand(
                Form.VendorName, null, Form.OrderDate, Form.Currency, Form.Remarks, Form.Lines,
                User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = $"Purchase order {result.Data.PoNumber} created as draft.";
            return RedirectToPage("Details", new { id = result.Data.Id });
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
