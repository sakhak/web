using AssetManagement.Application.Common;
using AssetManagement.Application.Contracts;
using AssetManagement.Application.Features.Maintenance;
using AssetManagement.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Maintenance.Schedules;

[Authorize]
public class IndexModel(ISender sender) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }

    public List<MaintenanceScheduleDto> Schedules { get; private set; } = [];

    public bool CanManage => User.GetUserPermAccess().Contains("MAINT_MANAGE");

    public async Task OnGetAsync()
    {
        var all = (await sender.Send(new GetMaintenanceSchedulesQuery(null))).Data ?? [];

        IEnumerable<MaintenanceScheduleDto> query = all;
        if (Status == "Active") query = query.Where(s => s.Status == AssetManagement.Domain.Constants.StatusCode.Active);
        if (Status == "Inactive") query = query.Where(s => s.Status != AssetManagement.Domain.Constants.StatusCode.Active);
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var term = SearchTerm.Trim();
            query = query.Where(s =>
                s.AssetNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                s.AssetName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                s.Title.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        Schedules = query.ToList();
    }
}
