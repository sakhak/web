using AssetManagement.Application.Common;
using AssetManagement.Application.Common.Constants;
using AssetManagement.Application.Contracts.Onboarding;
using AssetManagement.Application.Features.Depreciation;
using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Application.Contracts;
using AssetManagement.Application.Features.Assets;
using AssetManagement.Application.Features.Maintenance;
using AssetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Assets;

[Authorize(Policy = "ASSET_DETAIL")]
public class Details(ISender sender) : PageModel
{
    public Asset? Asset { get; set; }
    public AssetDepreciationDto? Depreciation { get; set; }
    public List<DepreciationProfileDto> Profiles { get; set; } = [];
    public List<MaintenanceScheduleDto> MaintenanceSchedules { get; set; } = [];
    public List<MaintenanceRecordDto> MaintenanceHistory { get; set; } = [];

    public bool CanManageMaintenance => User.GetUserPermAccess().Contains("MAINT_MANAGE");
    public bool CanCompleteMaintenance => User.GetUserPermAccess().Contains("MAINT_COMPLETE");

    public bool CanManageDepreciation => User.GetUserPermAccess().Contains("DEPRECIATION_MANAGE");

    public async Task<IActionResult> OnGetAsync(string? slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            HttpContext.Items[ApplicationConstants.LogKey.Description] = ApplicationConstants.MessageResponse.NotFound;
            return NotFound();
        }

        var result = await sender.Send(new GetAssetBySlugQuery(slug));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded) return NotFound();

        Asset = result.Data;
        await LoadDepreciationAsync();
        await LoadMaintenanceAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAssignProfileAsync(string slug, int profileId, DateTime? startDate)
    {
        if (!CanManageDepreciation) return Forbid();

        var asset = await sender.Send(new GetAssetBySlugQuery(slug));
        if (!asset.Succeeded) return NotFound();

        var result = await sender.Send(new AssignDepreciationProfileCommand(asset.Data.Id, profileId, startDate));
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Depreciation profile applied." : result.Message;
        return RedirectToPage(new { slug });
    }

    private async Task LoadMaintenanceAsync()
    {
        MaintenanceSchedules = (await sender.Send(new GetMaintenanceSchedulesQuery(Asset!.Id))).Data ?? [];
        MaintenanceHistory = (await sender.Send(new GetMaintenanceRecordsPagedQuery(new MaintenanceRecordFilter(AssetId: Asset.Id), 1, 5))).Data ?? [];
    }

    private async Task LoadDepreciationAsync()
    {
        Depreciation = (await sender.Send(new GetAssetDepreciationQuery(Asset!.Id))).Data;
        Profiles = (await sender.Send(new GetDepreciationProfilesQuery())).Data ?? [];
    }

    public IActionResult OnPostDeleteAsync(int id) => RedirectToPage("/Assets/Index");
}
