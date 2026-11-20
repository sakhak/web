using AssetManagement.Application.Common;
using AssetManagement.Application.Contracts;
using AssetManagement.Application.Features.Maintenance;
using AssetManagement.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Maintenance;

[Authorize]
public class IndexModel(ISender sender) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Department { get; set; }
    [BindProperty(SupportsGet = true)] public string? Location { get; set; }
    [BindProperty(SupportsGet = true)] public int? WithinDays { get; set; }

    public List<MaintenanceScheduleDto> Due { get; private set; } = [];
    public int OverdueCount => Due.Count(d => d.DueState == MaintenanceDueState.Overdue);
    public int DueSoonCount => Due.Count(d => d.DueState == MaintenanceDueState.DueSoon);

    public bool CanManage => User.GetUserPermAccess().Contains("MAINT_MANAGE");
    public bool CanComplete => User.GetUserPermAccess().Contains("MAINT_COMPLETE");

    public async Task OnGetAsync() =>
        Due = (await sender.Send(new GetMaintenanceDueQuery(Department, Location, WithinDays))).Data ?? [];
}
