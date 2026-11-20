using AssetManagement.Application.Contracts;
using AssetManagement.Application.Features.Users;
using AssetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Admin.UserManagement;

public class Index(ISender sender) : PageModel
{
    public List<User>? Users { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Claims.FirstOrDefault(x => x.Type == "Status")?.Value != "Y")
            return RedirectToPage("/Error", new { message = "User InActive" });

        var result = await sender.Send(new GetUsersQuery());
        Users = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateUserAsync([FromBody] User user)
    {
        var result = await sender.Send(new UpdateUserCommand(user));

        if (!result.Succeeded) return new JsonResult(new { success = false, messag = result.Message });
        return new JsonResult(new { success = true, message = result.Message });
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync([FromBody] UserStatusViewModel userStatus)
    {
        var branchCode = User.Claims.FirstOrDefault(c => c.Type == "BranchCode")?.Value;
        var staffId = User.Claims.FirstOrDefault(x => x.Type == "StaffId")?.Value;

        if (string.IsNullOrEmpty(branchCode)) return new JsonResult(new { success = false, messag = "Unknown BranchCode" });
        if (string.IsNullOrEmpty(staffId)) return new JsonResult(new { success = false, message = "Unknown StaffId" });

        var result = await sender.Send(new UpdateUserStatusCommand(userStatus));
        return new JsonResult(new { success = result.Succeeded, message = result.Message });
    }
}
