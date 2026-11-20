using AssetManagement.Application.Abstractions.Persistence;
using AssetManagement.Application.Features.Audit;
using AssetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Pages.Admin.AuditTrail;

[Authorize(Policy = "AUDIT_VIEW")]
public class IndexModel(ISender sender) : BasePageModel
{
    [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }
    [BindProperty(SupportsGet = true)] public string? UserName { get; set; }
    [BindProperty(SupportsGet = true)] public string? Module { get; set; }
    [BindProperty(SupportsGet = true)] public string? Method { get; set; }
    [BindProperty(SupportsGet = true)] public bool FailedOnly { get; set; }

    public List<SystemLog> Logs { get; set; } = [];
    public List<string> Modules { get; set; } = [];

    public bool HasFilter => From.HasValue || To.HasValue || !string.IsNullOrEmpty(UserName) || !string.IsNullOrEmpty(Module)
                             || !string.IsNullOrEmpty(Method) || !string.IsNullOrEmpty(SearchTerm) || FailedOnly;

    public async Task OnGetAsync()
    {
        if (PageSize is < 5 or > 100) PageSize = 25;
        if (PageNumber < 1) PageNumber = 1;

        var filter = new AuditLogFilter(From, To, UserName, Module, Method, SearchTerm, FailedOnly);
        var result = await sender.Send(new GetAuditLogsQuery(filter, PageNumber, PageSize));
        Logs = result.Data ?? [];
        TotalItems = result.TotalCount;
        Modules = await sender.Send(new GetAuditModulesQuery());
    }
}
