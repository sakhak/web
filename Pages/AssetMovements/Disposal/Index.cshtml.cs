using AssetManagement.Application.Common;
using AssetManagement.Application.Common.Constants;
using AssetManagement.Application.Features.Movements;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using static AssetManagement.Web.Models.ApprovalModel;

namespace AssetManagement.Web.Pages.AssetMovements.Disposal;

public class IndexModel(ISender sender) : PageModel
{
    [BindProperty]
    public IList<DisposalRecord>? DisposalRecords { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
    public int TotalItems { get; set; }
    public int TotalDisposalPending { get; set; }
    public int TotalDisposal { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public List<string> UserPermAccess { get; set; } = [];
    public bool IsHavePermAccess { get; set; }
    public List<SelectListItem> PageSizeOptions { get; } =
    [
        new() { Value = "1", Text = "1" },
        new() { Value = "2", Text = "2" },
        new() { Value = "5", Text = "5" },
        new() { Value = "10", Text = "10" },
        new() { Value = "25", Text = "25" },
        new() { Value = "50", Text = "50" }
    ];

    [BindProperty(SupportsGet = true)]
    public string? SearchBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }
    public DisposalApproval? DisposalApproval { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int pageSize = 10, int pageNumber = 1)
    {
        PageSize = pageSize;
        PageNumber = pageNumber;

        var paged = await sender.Send(new GetDisposalsPagedQuery(SearchBy, SearchTerm, pageNumber, pageSize));
        DisposalRecords = paged.Data;
        TotalItems = paged.TotalCount;

        var counts = await sender.Send(new GetDisposalCountsQuery(SearchBy, SearchTerm));
        if (counts.Succeeded && counts.Data is not null)
        {
            TotalDisposalPending = counts.Data.TotalDisposalPending;
            TotalDisposal = counts.Data.TotalDisposal;
        }

        UserPermAccess = User.GetUserPermAccess();
        IsHavePermAccess = UserPermAccess.Count != 0;

        HttpContext.Items[ApplicationConstants.LogKey.Description] = paged.Message;

        return Page();
    }

    public IActionResult OnGetApproveFormPartial(int[] ids)
    {
        DisposalApproval!.Id = ids[0];
        return Partial("~/Pages/AssetMovements/Disposal/_ApprovalForm.cshtml", DisposalApproval);
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostApprovalDisopsalAsync(DisposalApproval request)
    {
        var approvedByUser = User.Identity?.Name;

        var result = await sender.Send(new ApproveDisposalCommand(request.Id, request.Approved, request.Reason, approvedByUser));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded || result.Data is null)
        {
            if (result.Status == HttpStatus.NotFound)
                return NotFound(new { message = result.Message });

            return new JsonResult(new { success = false, message = result.Message });
        }

        return new JsonResult(new
        {
            success = true,
            message = result.Message,
            data = new { result.Data.Id, result.Data.Status, result.Data.ApprovedBy }
        });
    }
}
