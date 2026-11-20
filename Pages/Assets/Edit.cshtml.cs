using AssetManagement.Application.Common.Constants;
using AssetManagement.Application.Features.Assets;
using AssetManagement.Application.Features.Categories;
using AssetManagement.Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Assets;

public class Edit(ISender sender, IMapper mapper) : PageModel
{
    [BindProperty]
    public Asset? Asset { get; set; }

    public List<SelectListItem> CategoryItems { get; private set; } = [];

    private async Task LoadCategoriesAsync()
    {
        var categories = (await sender.Send(new GetAssetCategoriesQuery(ActiveOnly: false))).Data ?? [];
        CategoryItems = categories
            .Where(c => c.Status == "ACTIVE" || c.Id == Asset?.CategoryId)
            .Select(c => new SelectListItem($"{c.Code} - {c.Name}", c.Id.ToString())).ToList();
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var result = await sender.Send(new GetAssetByIdQuery(id.Value));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded) return NotFound();

        Asset = result.Data;
        await LoadCategoriesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCategoriesAsync();
        if (!ModelState.IsValid) return Page();
        if (Asset == null) return RedirectToPage("/Assets/Index");

        var result = await sender.Send(mapper.Map<UpdateAssetCommand>(Asset));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded)
        {
            ModelState.AddModelError("Internal_Error", result.Message);
            return Page();
        }

        return RedirectToPage("/Assets/Index");
    }
}
