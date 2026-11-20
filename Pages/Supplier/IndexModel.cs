using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Application.Features.Suppliers;
using AssetManagement.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplierEntity = AssetManagement.Domain.Entities.Supplier;

namespace AssetManagement.Web.Pages.Supplier;

[Authorize]
public class IndexModel(ISender sender) : BasePageModel
{
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }

    public List<SupplierEntity> Suppliers { get; set; } = [];
    public SupplierCounts Counts { get; set; } = new(0, 0, 0);

    public bool CanManage => UserPermAccess.Contains("SUPPLIER_MANAGE");

    public async Task OnGetAsync()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 10;

        BaseStatus? status = Enum.TryParse<BaseStatus>(Status, true, out var parsed) ? parsed : null;

        var result = await sender.Send(new GetSuppliersPagedQuery(SearchTerm, status, PageNumber, PageSize));
        Suppliers = result.Data ?? [];
        TotalItems = result.TotalCount;
        Counts = await sender.Send(new GetSupplierCountsQuery());
    }

    public async Task<IActionResult> OnPostToggleAsync(Guid id, bool active)
    {
        if (!CanManage) return Forbid();

        var result = await sender.Send(new SetSupplierStatusCommand(id, active, User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name));
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? $"{result.Data.FullName} is now {(active ? "active" : "inactive")}."
            : result.Message;

        return RedirectToPage();
    }
}
