using AssetManagement.Application.Abstractions.Services;
using AssetManagement.Application.Common;
using AssetManagement.Application.Contracts.Onboarding;
using AssetManagement.Application.Contracts.Reports;
using AssetManagement.Application.Features.Depreciation;
using AssetManagement.Application.Features.Reports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Reports;

[Authorize]
public class RunModel(ISender sender, IReportExporter exporter) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Key { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? AsOf { get; set; }
    [BindProperty(SupportsGet = true)] public string? Department { get; set; }
    [BindProperty(SupportsGet = true)] public string? Location { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public string? Type { get; set; }
    [BindProperty(SupportsGet = true)] public int? ProfileId { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public bool Print { get; set; }

    public ReportDefinition Definition { get; private set; } = null!;
    public ReportDocument Document { get; private set; } = new();
    public List<DepreciationProfileDto> Profiles { get; private set; } = [];
    public List<AssetManagement.Application.Contracts.AssetCategoryDto> Categories { get; private set; } = [];

    public ReportFilter Filter => new(From, To, AsOf, Department, Location, Status, Type, ProfileId, CategoryId);

    public async Task<IActionResult> OnGetAsync()
    {
        var denied = Authorize();
        if (denied is not null) return denied;

        var result = await Load();
        if (!result.Succeeded) return NotFound();

        Document = result.Data;
        if (Definition.Filters.HasFlag(ReportFilterFields.Profile))
            Profiles = (await sender.Send(new GetDepreciationProfilesQuery(ActiveOnly: false))).Data ?? [];

        if (Definition.Filters.HasFlag(ReportFilterFields.Category))
            Categories = (await sender.Send(new AssetManagement.Application.Features.Categories.GetAssetCategoriesQuery(ActiveOnly: false))).Data ?? [];

        return Page();
    }

    public async Task<IActionResult> OnGetExportAsync()
    {
        var denied = Authorize();
        if (denied is not null) return denied;

        var result = await Load();
        if (!result.Succeeded) return NotFound();

        var bytes = exporter.ToExcel(result.Data);
        var fileName = $"{Definition.Key}-{DateTime.Now:yyyyMMdd-HHmm}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private IActionResult? Authorize()
    {
        var definition = ReportCatalog.Find(Key);
        if (definition is null) return NotFound();
        if (!User.GetUserPermAccess().Contains(definition.Permission)) return Forbid();

        Definition = definition;
        return null;
    }

    private Task<AssetManagement.Application.Common.Models.Result<ReportDocument>> Load() =>
        sender.Send(new GetReportQuery(Definition.Key, Filter, User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name));
}
