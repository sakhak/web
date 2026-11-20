using System.Security.Claims;
using AssetManagement.Application.Features.Users;
using AssetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Admin.UserManagement.UserRole;

public class Index(ISender sender) : PageModel
{
    public List<User>? Users { get; set; }
    public List<Domain.Entities.Role>? Roles { get; set; }
    public List<Domain.Entities.UserRole>? UserRoles { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var staffId = User.Claims.FirstOrDefault(x => x.Type == "StaffId")?.Value;
        var role = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(staffId)) return RedirectToPage("/Error", new { message = "Unknown StaffId" });
        if (string.IsNullOrEmpty(role)) return RedirectToPage("/Error", new { message = "Unknown role" });

        if (User.Claims.FirstOrDefault(x => x.Type == "Status")?.Value != "Y")
            return RedirectToPage("/Error", new { message = "User InActive" });

        var result = await sender.Send(new GetAllUserRolesQuery(role));
        Users = result.Data.Users;
        Roles = result.Data.Roles;
        UserRoles = result.Data.UserRoles;

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateUserRoleAsync([FromBody] Domain.Entities.UserRole userRole)
    {
        var result = await sender.Send(new UpdateUserRoleCommand(userRole.UserId, userRole.RoleId));
        return new JsonResult(new { success = result.Succeeded, message = result.Message });
    }

    public async Task<IActionResult> OnPostDeleteUserRoleAsync([FromBody] List<Domain.Entities.UserRole> userRoles)
    {
        var result = await sender.Send(new RemoveUserRolesCommand(userRoles));
        return new JsonResult(new { success = result.Succeeded, message = result.Message });
    }
}
