using AssetManagement.Application.Contracts;
using AssetManagement.Application.Features.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Pages.AssetCategories;

[Authorize]
public class IndexModel(ISender sender) : BasePageModel
{
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }

    public List<AssetCategoryDto> Categories { get; set; } = [];
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
    public int AssignedAssets { get; set; }

    public bool CanManage => UserPermAccess.Contains("CATEGORY_MANAGE");

    public async Task OnGetAsync()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 10;

        var statusFilter = Status switch { "Active" => "ACTIVE", "Inactive" => "INACTIVE", _ => null };
        var result = await sender.Send(new GetAssetCategoriesPagedQuery(SearchTerm, statusFilter, PageNumber, PageSize));
        Categories = result.Data ?? [];
        TotalItems = result.TotalCount;

        var all = (await sender.Send(new GetAssetCategoriesQuery(ActiveOnly: false))).Data ?? [];
        ActiveCount = all.Count(c => c.Status == "ACTIVE");
        InactiveCount = all.Count - ActiveCount;
        AssignedAssets = all.Sum(c => c.AssetCount);
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        if (!CanManage) return Forbid();

        var current = await sender.Send(new GetAssetCategoryByIdQuery(id));
        if (!current.Succeeded) return NotFound();

        var c = current.Data;
        var result = await sender.Send(new UpdateAssetCategoryCommand(
            c.Id, c.Name, c.Description, c.DefaultProfileId, c.Status != "ACTIVE",
            User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name));

        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? $"Category {c.Code} is now {(result.Data.Status == "ACTIVE" ? "active" : "inactive")}."
            : result.Message;

        return RedirectToPage();
    }
}
