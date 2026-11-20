using AssetManagement.Application.Common;
using AssetManagement.Application.Common.Constants;
using AssetManagement.Application.Contracts;
using AssetManagement.Application.Features.Assets;
using AssetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssetManagement.Web.Pages.Assets;

public class IndexModel(ISender sender) : PageModel
{
    [BindProperty]
    public List<Asset>? Assets { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public List<string> UserPermAccess { get; set; } = [];
    public bool IsHavePermAccess { get; set; }
    public List<SelectListItem> PageSizeOptions { get; } =
    [
        new() { Value = "1", Text = "1" },
        new() { Value = "2", Text = "2" },
        new() { Value = "5", Text = "5" },
        new() { Value = "10", Text = "10" },
        new() { Value = "25", Text = "25" },
        new() { Value = "50", Text = "50" }
    ];

    [BindProperty(SupportsGet = true)]
    public string? SearchBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public TotalAssetsViewModel TotalAssets { get; set; } = new();

    public Dictionary<int, string> CategoryNames { get; private set; } = [];
    public List<AssetManagement.Application.Contracts.AssetCategoryDto> Categories { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int pageSize = 10, int pageNumber = 1)
    {
        PageSize = pageSize;
        PageNumber = pageNumber;

        var summary = await sender.Send(new GetAssetSummaryQuery());
        if (summary.Succeeded) TotalAssets = summary.Data;

        Categories = (await sender.Send(new AssetManagement.Application.Features.Categories.GetAssetCategoriesQuery(ActiveOnly: false))).Data ?? [];
        CategoryNames = Categories.ToDictionary(c => c.Id, c => c.Name);

        var paged = await sender.Send(new GetAssetsPagedQuery(SearchBy, SearchTerm, pageNumber, pageSize));
        Assets = paged.Data;
        TotalItems = paged.TotalCount;

        UserPermAccess = User.GetUserPermAccess();
        IsHavePermAccess = UserPermAccess.Count != 0;

        HttpContext.Items[ApplicationConstants.LogKey.Description] = paged.Message;
        return Page();
    }
}
