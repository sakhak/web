using AssetManagement.Application.Features.CategoryRoles;
using AssetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static AssetManagement.Application.Common.Constants.ApplicationConstants;

namespace AssetManagement.Web.Pages.Admin.UserManagement.CategoryRole;

public class IndexModel(ISender sender) : PageModel
{
    public List<EmpCategoryRole>? CategoryRoles { get; set; }
    public List<Domain.Entities.Role>? Roles { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Claims.FirstOrDefault(x => x.Type == "Status")?.Value != "Y")
            return RedirectToPage("/Error", new { message = "User InActive" });

        var staffId = User.Claims.FirstOrDefault(x => x.Type == "StaffId")?.Value;
        if (string.IsNullOrEmpty(staffId))
        {
            TempData["mask-error"] = "Staff Id not found";
            return Page();
        }

        var result = await sender.Send(new GetCategoryRolesQuery());
        CategoryRoles = result.Data.CategoryRoles;
        Roles = result.Data.Roles;

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateCategoryRoleAsync([FromBody] EmpCategoryRole categoryRole)
    {
        var staffId = User.Claims.FirstOrDefault(x => x.Type == ClaimType.StaffId)?.Value;
        if (string.IsNullOrEmpty(staffId)) return new JsonResult(new { success = false, message = "Invalid Staff Id" });

        var result = await sender.Send(new UpdateCategoryRoleCommand(staffId, categoryRole));
        return new JsonResult(new { success = result.Succeeded, message = result.Message });
    }

    public async Task<IActionResult> OnPostCreateCategoryRoleAsync([FromBody] EmpCategoryRole categoryRole)
    {
        var staffId = User.Claims.FirstOrDefault(x => x.Type == ClaimType.StaffId)?.Value;
        if (string.IsNullOrEmpty(staffId)) return new JsonResult(new { success = false, message = "Invalid Staff Id" });

        var result = await sender.Send(new AddCategoryRoleCommand(staffId, categoryRole));
        return new JsonResult(new { success = true, message = result.Message, data = result.Data });
    }

    public async Task<IActionResult> OnPostDeleteCategoryRoleAsync([FromBody] List<EmpCategoryRole> categoryRole)
    {
        var staffId = User.Claims.FirstOrDefault(x => x.Type == ClaimType.StaffId)?.Value;
        if (string.IsNullOrEmpty(staffId)) return new JsonResult(new { success = false, message = "Invalid Staff Id" });

        var result = await sender.Send(new DeleteCategoryRolesCommand(staffId, categoryRole));
        return new JsonResult(new { success = result.Succeeded, message = result.Message });
    }
}
