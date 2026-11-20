using AssetManagement.Application.Common.Constants;
using AssetManagement.Application.Features.Movements;
using AssetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.AssetMovements;

public class Accept(ISender sender) : PageModel
{
    [BindProperty]
    public string TrackingCode { get; set; } = string.Empty;

    [BindProperty]
    public string AcceptedBy { get; set; } = string.Empty;

    [BindProperty]
    public string? Comments { get; set; }

    public AssetMovement? AssetMovement { get; set; }

    public Asset? Asset { get; set; }

    public bool SearchPerformed { get; set; }

    public bool AcceptanceCompleted { get; set; }

    public void OnGet(string? code = null)
    {
        SearchPerformed = false;
        AcceptanceCompleted = false;

        if (!string.IsNullOrEmpty(code)) TrackingCode = code;
    }

    public async Task<IActionResult> OnPostSearchAsync()
    {
        SearchPerformed = true;

        if (string.IsNullOrEmpty(TrackingCode)) return Page();

        var result = await sender.Send(new GetMovementByTrackingCodeQuery(TrackingCode));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (result.Succeeded)
        {
            AssetMovement = result.Data;
            Asset = result.Data?.Asset;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAcceptAsync()
    {
        if (string.IsNullOrEmpty(TrackingCode) || string.IsNullOrEmpty(AcceptedBy))
        {
            ModelState.AddModelError(string.Empty, "Tracking code and your name are required.");
            return await OnPostSearchAsync();
        }

        var result = await sender.Send(new AcceptMovementCommand(TrackingCode, AcceptedBy, Comments));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return await OnPostSearchAsync();
        }

        SearchPerformed = true;
        AcceptanceCompleted = true;
        AssetMovement = result.Data;
        Asset = result.Data.Asset;

        return Page();
    }
}
