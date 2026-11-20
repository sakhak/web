using AssetManagement.Application.Abstractions.Services;
using AssetManagement.Application.Common.Constants;
using AssetManagement.Application.Contracts;
using AssetManagement.Application.Features.Assets;
using AssetManagement.Application.Features.Movements;
using AssetManagement.Application.Features.Settings;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Rules;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.AssetMovements;

public class CreateModel(
    ISender sender,
    IMapper mapper,
    IHrisEmployeeService hrisEmployeeService,
    ILogger<CreateModel> logger) : PageModel
{
    [BindProperty]
    public AssetMovement? AssetMovement { get; set; }

    [BindProperty]
    public RepairRecord? RepairRecord { get; set; }

    [BindProperty]
    public DisposalRecord? DisposalRecord { get; set; }

    [BindProperty]
    public required string MovementType { get; set; }

    [BindProperty]
    public bool ApprovalRequired { get; set; } = true;
    public Asset? Asset { get; set; }
    [BindProperty]
    public PermOfMovement? PermOfMovement { get; set; }
    [BindProperty]
    public bool IsDeposed { get; set; } = false;
    [BindProperty]
    public bool IsPendingApproval { get; set; } = false;
    public bool IsMovementPending { get; set; } = false;
    public List<Setting> Departments { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var assetResult = await sender.Send(new GetAssetBySlugQuery(slug));

        HttpContext.Items[ApplicationConstants.LogKey.Description] = assetResult.Message;

        if (!assetResult.Succeeded || assetResult.Data is null) return NotFound();

        Asset = assetResult.Data;

        Departments = (await sender.Send(new GetSettingsByTypeCodeQuery(SettingTypeCode.Department))).Data ?? [];

        var createContext = await sender.Send(new GetMovementCreateContextQuery(Asset.Id));
        if (createContext.Succeeded && createContext.Data is not null)
        {
            AssetMovement = createContext.Data.AssetMovement;
            RepairRecord = createContext.Data.RepairRecord;
            DisposalRecord = createContext.Data.DisposalRecord;
            IsMovementPending = createContext.Data.IsMovementPending;
            IsPendingApproval = createContext.Data.IsPendingApproval;
            ApprovalRequired = createContext.Data.ApprovalRequired;
        }
        else
        {
            AssetMovement = new AssetMovement { AssetId = Asset.Id, CurrentCustodian = "Current User" };
            RepairRecord = new RepairRecord { AssetId = Asset.Id };
            DisposalRecord = new DisposalRecord { AssetId = Asset.Id };
        }

        if (AssetMovement.Id == 0)
            AssetMovement.SourceLocation = Asset.Location;

        PermOfMovement = MovementRules.ResolveAllowedMovementTypes(Asset.Status);

        IsDeposed = !PermOfMovement.Transter && !PermOfMovement.Repair && !PermOfMovement.Deposal;

        return Page();
    }

    public async Task<JsonResult> OnGetLookupStaffAsync(string staffId)
    {
        if (string.IsNullOrWhiteSpace(staffId))
            return new JsonResult(new { found = false, message = "staff Not found" });

        try
        {
            var employee = await hrisEmployeeService.GetEmployeeInfoAsync(staffId);

            if (employee is null)
                return new JsonResult(new { found = false, message = "staff Not found" });

            var fullName = $"{employee.Courtesy}{employee.LastName} {employee.FirstName}".Trim();
            var text = $"{fullName} — {employee.BranchCode} — {employee.PostTitle}";

            return new JsonResult(new { found = true, text, fullName, email = employee.Email ?? string.Empty });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LookupStaff failed staffId={StaffId}", staffId);
            return new JsonResult(new { found = false, message = "staff Not found" });
        }
    }

    public async Task<IActionResult> OnPostAsync(string slug)
    {
        if (!ModelState.IsValid)
        {
            foreach (var (key, state) in ModelState)
                foreach (var error in state.Errors)
                    logger.LogWarning("ModelState error on '{Key}': {ErrorMessage}", key, error.ErrorMessage);

            Asset = (await sender.Send(new GetAssetBySlugQuery(slug))).Data;
            Departments = (await sender.Send(new GetSettingsByTypeCodeQuery(SettingTypeCode.Department))).Data ?? [];

            return Page();
        }

        var staffId = User.Claims.FirstOrDefault(x => x.Type == "StaffId")?.Value;

        var result = await sender.Send(new CreateMovementCommand
        {
            Movement = mapper.Map<MovementInput>(AssetMovement),
            MovementType = MovementType,
            ApprovalRequired = ApprovalRequired,
            ApprovedBy = User.FindFirst("DisplayName")?.Value ?? User.Identity?.Name ?? staffId ?? "Unknown",
            Repair = RepairRecord is null ? null : mapper.Map<RepairInput>(RepairRecord),
            Disposal = DisposalRecord is null ? null : mapper.Map<DisposalInput>(DisposalRecord),
            StaffId = staffId
        });

        HttpContext.Items[ApplicationConstants.LogKey.Description] = result.Message;

        if (!result.Succeeded || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Message);

            Asset = (await sender.Send(new GetAssetByIdQuery(AssetMovement!.AssetId))).Data;
            if (Asset is null) return NotFound();

            return Page();
        }

        return RedirectToPage("./Details", new { id = result.Data.Id });
    }
}
