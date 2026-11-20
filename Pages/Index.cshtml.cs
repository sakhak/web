using AssetManagement.Application.Abstractions.Services;
using AssetManagement.Application.Common.Mappings;
using AssetManagement.Application.Features.Searches;
using AssetManagement.Application.Features.Users;
using AssetManagement.Application.Abstractions.Models;
using AssetManagement.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;

namespace AssetManagement.Web.Pages;

[AuthorizeForScopes(Scopes = ["User.Read"])]
public class IndexModel(
    ISender sender,
    IConfiguration configuration,
    IWebHostEnvironment environment,
    ITokenAcquisition tokenAcquisition,
    IAzureEmployeeService azureEmployeeService,
    IHrisEmployeeService hrisEmployeeService,
    ILogger<IndexModel> logger) : PageModel
{
    private readonly string? _apiScope = configuration["DownstreamApis:Scopes"];

    public EmployeeAzure? EmployeeInfo { get; set; }
    public EmployeeHris? EmployeeHris { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (AuthenticationExtensions.IsLocalLoginEnabled(configuration, environment))
            return Redirect("/Dashboard/Index");

        if (_apiScope != null)
        {
            var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync([_apiScope]);

            var email = User.Identity?.Name;
            EmployeeHris = new EmployeeHris()
            {
                StaffId = "112233",
                FirstName = "test 1",
                LastName = "22",
                Email = "test@local.com.kh"
            };
            //if (!string.IsNullOrEmpty(email))
            //    EmployeeInfo = await azureEmployeeService.GetEmployeeInfoAsync(email, accessToken);

            //if (EmployeeInfo is { EmployeeId: not null })
            //    EmployeeHris = await hrisEmployeeService.GetEmployeeInfoAsync(EmployeeInfo.EmployeeId);

            if (email != null)
            {
                email = email.Trim();
                var user = EmployeeHris?.ToUserFromHrisDto(EmployeeInfo?.DisplayName, email)
                           ?? throw new ArgumentNullException(nameof(EmployeeHris), "User cannot be null.");

                var result = await sender.Send(new UpsertUserCommand(user));
                if (!result.Succeeded)
                    return RedirectToPage("/Error", new { message = result.Message });
            }
        }

        return Redirect("/Dashboard/Index");
    }

    public async Task<IActionResult> OnGetSearchItemAsync(string searchItem)
    {
        try
        {
            var result = await sender.Send(new SearchItemsQuery(searchItem));
            return new JsonResult(new { success = true, data = result, message = "Success" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Search failed for {Term}", searchItem);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }
}
