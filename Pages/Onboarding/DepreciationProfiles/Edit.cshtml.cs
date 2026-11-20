using AssetManagement.Application.Features.Depreciation;
using AssetManagement.Web.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Onboarding.DepreciationProfiles;

[Authorize(Policy = "DEPRECIATION_MANAGE")]
public class EditModel(ISender sender) : PageModel
{
    [BindProperty]
    public DepreciationProfileForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var profile = (await sender.Send(new GetDepreciationProfilesQuery(ActiveOnly: false))).Data?.FirstOrDefault(p => p.Id == id);
        if (profile is null) return NotFound();

        Form = new DepreciationProfileForm
        {
            Id = profile.Id,
            Code = profile.Code,
            Name = profile.Name,
            UsefulLifeMonths = profile.UsefulLifeMonths,
            SalvagePercent = profile.SalvagePercent,
            Active = profile.Status == "ACTIVE"
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            var result = await sender.Send(new UpdateDepreciationProfileCommand(
                id, Form.Name, Form.UsefulLifeMonths, Form.SalvagePercent, Form.Active,
                User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = $"Profile {result.Data.Code} updated.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            ModelState.AddFluentErrors(ex, nameof(Form));
            return Page();
        }
    }
}
