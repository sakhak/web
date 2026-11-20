using AssetManagement.Application.Contracts;
using AssetManagement.Application.Features.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Dashboard;

[Authorize]
public class IndexModel(ISender sender) : PageModel
{
    public DashboardDto Data { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var result = await sender.Send(new GetDashboardQuery());
        if (result.Succeeded) Data = result.Data;
    }
}
