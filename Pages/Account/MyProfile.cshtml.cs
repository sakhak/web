using AssetManagement.Application.Features.Users;
using AssetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.Web.Pages.Account;

public class MyProfile(ISender sender) : PageModel
{
    public User? UserInfo { get; set; }

    public async Task OnGet()
    {
        var email = User.Identity?.Name;
        if (email == null) return;

        UserInfo = await sender.Send(new GetUserByEmailQuery(email));
    }
}
