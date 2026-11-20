using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Application.Common;
using AssetManagement.Application.Contracts;
using AssetManagement.Application.Features.Maintenance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Pages.Maintenance.Records;

[Authorize]
public class IndexModel(ISender sender) : BasePageModel
{
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public string? Type { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }

    public List<MaintenanceRecordDto> Records { get; private set; } = [];

    public bool CanManage => UserPermAccess.Contains("MAINT_MANAGE");
    public bool CanComplete => UserPermAccess.Contains("MAINT_COMPLETE");

    public bool HasFilter => !string.IsNullOrEmpty(Status) || !string.IsNullOrEmpty(Type) || From.HasValue || To.HasValue || !string.IsNullOrEmpty(SearchTerm);

    public async Task OnGetAsync()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 5 || PageSize > 100) PageSize = 20;

        var filter = new MaintenanceRecordFilter(null, Status, Type, From, To, SearchTerm);
        var result = await sender.Send(new GetMaintenanceRecordsPagedQuery(filter, PageNumber, PageSize));
        Records = result.Data ?? [];
        TotalItems = result.TotalCount;
    }
}
