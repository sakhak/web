using AssetManagement.Application.Contracts.Onboarding;
using AssetManagement.Application.Features.Depreciation;
using AssetManagement.Application.Features.Onboarding;
using AssetManagement.Application.Features.Settings;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using AssetManagement.Web.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Onboarding.GoodsReceipts;

[Authorize(Policy = "ASSET_TAG")]
public class TagModel(ISender sender) : PageModel
{
    [BindProperty]
    public TagAssetsForm Form { get; set; } = new();

    public GoodsReceiptDto Receipt { get; set; } = new();
    public GoodsReceiptLineDto Line { get; set; } = new();
    public List<DepreciationProfileDto> Profiles { get; set; } = [];
    public List<Setting> Departments { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int grnId, int lineId)
    {
        if (!await LoadAsync(grnId, lineId)) return NotFound();

        if (Line.RemainingToTag <= 0)
        {
            TempData["Error"] = "All received units of this line are already tagged.";
            return RedirectToPage("Details", new { id = grnId });
        }

        var profileId = Line.DepreciationProfileId;
        Form = new TagAssetsForm
        {
            DepreciationProfileId = profileId,
            StartDate = Receipt.ReceivedDate.Date,
            Assets = Enumerable.Range(0, Math.Min(Line.RemainingToTag, 5)).Select(_ => new TagAssetInput()).ToList()
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int grnId, int lineId)
    {
        if (!await LoadAsync(grnId, lineId)) return NotFound();

        ModelState.Clear();
        var assets = Form.Assets.Where(a => !string.IsNullOrWhiteSpace(a.AssetNumber)).ToList();
        if (assets.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Enter at least one asset number.");
            return Page();
        }

        try
        {
            var result = await sender.Send(new TagAssetsFromGoodsReceiptCommand(
                lineId, assets, Form.DepreciationProfileId, Form.StartDate));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["Success"] = $"{result.Data.Count} asset(s) created from {Receipt.GrnNumber}.";
            return RedirectToPage("Details", new { id = grnId });
        }
        catch (ValidationException ex)
        {
            ModelState.AddFluentErrors(ex, nameof(Form));
            return Page();
        }
    }

    private async Task<bool> LoadAsync(int grnId, int lineId)
    {
        var result = await sender.Send(new GetGoodsReceiptByIdQuery(grnId));
        var line = result.Data?.Lines.FirstOrDefault(l => l.Id == lineId);
        if (!result.Succeeded || line is null) return false;

        Receipt = result.Data;
        Line = line;
        Profiles = (await sender.Send(new GetDepreciationProfilesQuery())).Data ?? [];
        Departments = (await sender.Send(new GetSettingsByTypeCodeQuery(SettingTypeCode.Department))).Data ?? [];
        return true;
    }
}
