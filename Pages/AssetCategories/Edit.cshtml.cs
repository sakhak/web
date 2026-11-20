using AssetManagement.Application.Contracts.Onboarding;
using AssetManagement.Application.Features.Categories;
using AssetManagement.Application.Features.Depreciation;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Enums;
using AssetManagement.Web.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.AssetCategories;

[Authorize(Policy = "CATEGORY_MANAGE")]
public class EditModel(ISender sender) : PageModel
{
    [BindProperty]
    public AssetCategoryForm Form { get; set; } = new();

    public List<DepreciationProfileDto> Profiles { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await sender.Send(new GetAssetCategoryByIdQuery(id));
        if (!result.Succeeded) return NotFound();

        var c = result.Data;
        Form = new AssetCategoryForm
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            Description = c.Description,
            DefaultProfileId = c.DefaultProfileId,
            Active = c.Status == AssetManagement.Domain.Constants.StatusCode.Active
        };

        await LoadProfilesAsync(c.DefaultProfileId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await LoadProfilesAsync(Form.DefaultProfileId);
        if (!ModelState.IsValid) return Page();

        try
        {
            var result = await sender.Send(new UpdateAssetCategoryCommand(
                id, Form.Name, Form.Description, Form.DefaultProfileId, Form.Active,
                User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name));

            if (result.Status == HttpStatus.NotFound) return NotFound();
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = $"Category {result.Data.Code} updated.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            ModelState.AddFluentErrors(ex, nameof(Form));
            return Page();
        }
    }

    private async Task LoadProfilesAsync(int? keepProfileId)
    {
        var all = (await sender.Send(new GetDepreciationProfilesQuery(ActiveOnly: false))).Data ?? [];
        Profiles = all.Where(p => p.Status == AssetManagement.Domain.Constants.StatusCode.Active || p.Id == keepProfileId).ToList();
    }
}
