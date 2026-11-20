using AssetManagement.Application.Common;
using AssetManagement.Application.Contracts.Onboarding;
using AssetManagement.Application.Features.Depreciation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Onboarding.DepreciationProfiles;

[Authorize]
public class IndexModel(ISender sender) : PageModel
{
    public List<DepreciationProfileDto> Profiles { get; set; } = [];

    public bool CanManage => User.GetUserPermAccess().Contains("DEPRECIATION_MANAGE");

    public async Task OnGetAsync() =>
        Profiles = (await sender.Send(new GetDepreciationProfilesQuery(ActiveOnly: false))).Data ?? [];
}
