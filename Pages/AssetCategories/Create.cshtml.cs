using AssetManagement.Application.Contracts.Onboarding;
using AssetManagement.Application.Features.Categories;
using AssetManagement.Application.Features.Depreciation;
using AssetManagement.Web.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.AssetCategories;

[Authorize(Policy = "CATEGORY_MANAGE")]
public class CreateModel(ISender sender) : PageModel
{
    [BindProperty]
    public AssetCategoryForm Form { get; set; } = new();

    public List<DepreciationProfileDto> Profiles { get; private set; } = [];

    public async Task OnGetAsync() => await LoadProfilesAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadProfilesAsync();
        if (!ModelState.IsValid) return Page();

        try
        {
            var result = await sender.Send(new CreateAssetCategoryCommand(
                Form.Code, Form.Name, Form.Description, Form.DefaultProfileId,
                User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = $"Category {result.Data.Code} created.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            ModelState.AddFluentErrors(ex, nameof(Form));
            return Page();
        }
    }

    private async Task LoadProfilesAsync() =>
        Profiles = (await sender.Send(new GetDepreciationProfilesQuery())).Data ?? [];
}
