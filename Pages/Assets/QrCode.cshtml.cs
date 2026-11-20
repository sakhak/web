using AssetManagement.Application.Features.Assets;
using AssetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Assets;

public class QrCode(ISender sender) : PageModel
{
    public Asset? Asset { get; set; }
    public string? QrCodeImageBase64 { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await sender.Send(new GetAssetQrCodeQuery(id));
        if (!result.Succeeded) return NotFound();

        Asset = result.Data.Asset;
        QrCodeImageBase64 = result.Data.QrCodeBase64;
        return Page();
    }
}
