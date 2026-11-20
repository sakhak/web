using AssetManagement.Application.Contracts.Onboarding;
using AssetManagement.Application.Features.Onboarding;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Pages.Onboarding.GoodsReceipts;

[Authorize]
public class IndexModel(ISender sender) : BasePageModel
{
    public List<GoodsReceiptDto> Receipts { get; set; } = [];

    public async Task OnGetAsync()
    {
        var result = await sender.Send(new GetGoodsReceiptsPagedQuery(SearchTerm, null, PageNumber, PageSize));
        Receipts = result.Data ?? [];
        TotalItems = result.TotalCount;
    }
}
