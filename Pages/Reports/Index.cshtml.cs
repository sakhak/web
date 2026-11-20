using AssetManagement.Application.Common;
using AssetManagement.Application.Contracts.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Reports;

[Authorize]
public class IndexModel : PageModel
{
    public List<ReportDefinition> Available { get; private set; } = [];

    public void OnGet()
    {
        var permissions = User.GetUserPermAccess();
        Available = ReportCatalog.All.Where(r => permissions.Contains(r.Permission)).ToList();
    }
}
