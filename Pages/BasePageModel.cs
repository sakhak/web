using AssetManagement.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssetManagement.Web.Pages;

public abstract class BasePageModel : PageModel
{
    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string? SearchBy { get; set; }
    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
    [BindProperty(SupportsGet = true)] public string? SortBy { get; set; }
    [BindProperty(SupportsGet = true)] public string? SortOrder { get; set; } = "asc";

    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    public List<string> UserPermAccess { get; private set; } = [];
    public bool IsHavePermAccess { get; private set; }

    public List<SelectListItem> PageSizeOptions { get; } =
    [
        new() { Value = "5", Text = "5" },
        new() { Value = "10", Text = "10" },
        new() { Value = "25", Text = "25" },
        new() { Value = "50", Text = "50" }
    ];

    public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
    {
        UserPermAccess = User.GetUserPermAccess();
        IsHavePermAccess = UserPermAccess.Count != 0;
        base.OnPageHandlerExecuting(context);
    }
}
