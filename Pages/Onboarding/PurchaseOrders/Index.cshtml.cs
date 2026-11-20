using AssetManagement.Application.Contracts.Onboarding;
using AssetManagement.Application.Features.Onboarding;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Pages.Onboarding.PurchaseOrders;

[Authorize]
public class IndexModel(ISender sender) : BasePageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    public List<PurchaseOrderDto> Orders { get; set; } = [];

    public async Task OnGetAsync()
    {
        var result = await sender.Send(new GetPurchaseOrdersPagedQuery(SearchTerm, Status, PageNumber, PageSize));
        Orders = result.Data ?? [];
        TotalItems = result.TotalCount;
    }
}
