using AssetManagement.Application.Common;
using AssetManagement.Application.Contracts.Onboarding;
using AssetManagement.Application.Features.Onboarding;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Onboarding.GoodsReceipts;

[Authorize]
public class DetailsModel(ISender sender) : PageModel
{
    public GoodsReceiptDto Receipt { get; set; } = new();

    public bool CanTag => User.GetUserPermAccess().Contains("ASSET_TAG");

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await sender.Send(new GetGoodsReceiptByIdQuery(id));
        if (!result.Succeeded) return NotFound();

        Receipt = result.Data;
        return Page();
    }
}
