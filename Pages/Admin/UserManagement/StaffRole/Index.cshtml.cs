using AssetManagement.Application.Features.StaffRoles;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static AssetManagement.Application.Common.Constants.ApplicationConstants;

namespace AssetManagement.Web.Pages.Admin.UserManagement.StaffRole;

public class Index(ISender sender) : PageModel
{
    public List<Domain.Entities.StaffRole>? StaffRoles { get; set; }
    public List<Domain.Entities.Role>? Roles { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Claims.FirstOrDefault(x => x.Type == "Status")?.Value != "Y")
            return RedirectToPage("/Error", new { message = "User InActive" });

        var result = await sender.Send(new GetStaffRolesQuery());
        StaffRoles = result.Data.StaffRoles;
        Roles = result.Data.Roles;

        return Page();
    }

    public async Task<IActionResult> OnPostCreateStaffRoleAsync([FromBody] Domain.Entities.StaffRole staffRole)
    {
        var staffId = User.Claims.FirstOrDefault(x => x.Type == ClaimType.StaffId)?.Value;
        if (string.IsNullOrEmpty(staffId)) return new JsonResult(new { success = false, message = "Invalid Staff Id" });

        var result = await sender.Send(new AddStaffRoleCommand(staffId, staffRole));
        if (!result.Succeeded) return new JsonResult(new { success = false, message = result.Message });

        return new JsonResult(new { success = true, message = "created successfully", data = result.Data });
    }

    public async Task<IActionResult> OnPostDeleteStaffRoleAsync([FromBody] List<Domain.Entities.StaffRole> staffRole)
    {
        var staffId = User.Claims.FirstOrDefault(x => x.Type == ClaimType.StaffId)?.Value;
        if (string.IsNullOrEmpty(staffId)) return new JsonResult(new { success = false, message = "Invalid Staff Id" });

        var result = await sender.Send(new DeleteStaffRolesCommand(staffId, staffRole));
        return new JsonResult(new { success = result.Succeeded, message = result.Message });
    }

    public async Task<IActionResult> OnPostUpdateStaffRoleAsync([FromBody] Domain.Entities.StaffRole staffRole)
    {
        var staffId = User.Claims.FirstOrDefault(x => x.Type == ClaimType.StaffId)?.Value;
        if (string.IsNullOrEmpty(staffId)) return new JsonResult(new { success = false, message = "Invalid Staff Id" });

        var result = await sender.Send(new UpdateStaffRoleCommand(staffId, staffRole));
        return new JsonResult(new { success = result.Succeeded, message = result.Message });
    }
}
