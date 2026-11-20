using AssetManagement.Application.Features.Roles;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Admin.UserManagement.Role;

public class Index(ISender sender) : PageModel
{
    public List<Domain.Entities.Role>? Role { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Claims.FirstOrDefault(x => x.Type == "Status")?.Value != "Y")
            return RedirectToPage("/Error", new { message = "User InActive" });

        var result = await sender.Send(new GetRolesQuery());
        TempData["mask-success"] = result.Message;
        Role = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateRoleAsync([FromBody] Domain.Entities.Role roleItem)
    {
        if (roleItem == null) return BadRequest(new { success = false, message = "No data to update" });

        var result = await sender.Send(new UpdateRoleCommand(roleItem.Id, roleItem.Name, roleItem.Remark));
        return new JsonResult(new { success = result.Succeeded, message = result.Message });
    }

    public async Task<IActionResult> OnPostAddRoleAsync([FromBody] Domain.Entities.Role roleItem)
    {
        if (roleItem == null) return BadRequest(new { success = false, message = "No data to add" });

        var result = await sender.Send(new AddRoleCommand(roleItem.Name, roleItem.Remark));
        if (!result.Succeeded) return new JsonResult(new { success = false, message = result.Message });

        return new JsonResult(new { success = true, data = result.Data });
    }

    public async Task<IActionResult> OnPostDeleteRoleAsync([FromBody] List<Domain.Entities.Role> roleItems)
    {
        if (!roleItems.Any()) return BadRequest(new { success = false, message = "No data to delete" });

        var result = await sender.Send(new DeleteRolesCommand(roleItems));
        return new JsonResult(new { success = result.Succeeded, message = result.Message });
    }
}
