using AssetManagement.Application.Features.Depreciation;
using AssetManagement.Web.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Onboarding.DepreciationProfiles;

[Authorize(Policy = "DEPRECIATION_MANAGE")]
public class CreateModel(ISender sender) : PageModel
{
    [BindProperty]
    public DepreciationProfileForm Form { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            var result = await sender.Send(new CreateDepreciationProfileCommand(
                Form.Code, Form.Name, Form.UsefulLifeMonths, Form.SalvagePercent,
                User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = $"Profile {result.Data.Code} created.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            ModelState.AddFluentErrors(ex, nameof(Form));
            return Page();
        }
    }
}
