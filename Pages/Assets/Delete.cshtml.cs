using AssetManagement.Application.Common.Constants;
using AssetManagement.Application.Features.Assets;
using AssetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Assets;

public class Delete(ISender sender) : PageModel
{
    [BindProperty]
    public Asset? Asset { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var result = await sender.Send(new GetAssetByIdQuery(id.Value));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded) return NotFound();

        Asset = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();

        var result = await sender.Send(new DeleteAssetCommand(id.Value));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded)
        {
            var reload = await sender.Send(new GetAssetByIdQuery(id.Value));
            Asset = reload.Data;
            ModelState.AddModelError("Internal_Error", result.Message);
            return Page();
        }

        return RedirectToPage("/Assets/Index");
    }
}
