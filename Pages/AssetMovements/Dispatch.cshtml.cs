using AssetManagement.Application.Common.Constants;
using AssetManagement.Application.Features.Movements;
using AssetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.AssetMovements;

public class DispatchModel(ISender sender) : PageModel
{
    [BindProperty]
    public AssetMovement? AssetMovement { get; set; }

    [BindProperty]
    public string? Comments { get; set; }

    public Asset? Asset { get; set; }

    public string? QrCodeImageBase64 { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var result = await sender.Send(new GetMovementForDispatchQuery(id.Value));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded || result.Data is null) return NotFound();

        Load(result.Data);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await sender.Send(new DispatchMovementCommand(AssetMovement!.Id, Comments));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);

            var reload = await sender.Send(new GetMovementForDispatchQuery(AssetMovement.Id));
            if (reload.Succeeded) Load(reload.Data);
            else AssetMovement = null;

            return Page();
        }

        return RedirectToPage("./Details", new { id = AssetMovement.Id });
    }

    private void Load(AssetManagement.Application.Contracts.DispatchViewModel data)
    {
        AssetMovement = data.Movement;
        Asset = data.Movement.Asset;
        QrCodeImageBase64 = data.QrCodeBase64;
    }
}
