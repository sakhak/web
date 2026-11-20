using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using Serilog;

namespace AssetManagement.Web.Pages
{

    [AuthorizeForScopes(Scopes = ["User.Read"])]
    public class AzureCallbackModel(
        ITokenAcquisition tokenAcquisition,
        IConfiguration config
    ) : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            var apiScope = config["DownstreamApis:Scopes"];

            try
            {
                if (!string.IsNullOrEmpty(apiScope))
                {
                    var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(
                        new[] { apiScope },
                        user: User,
                        authenticationScheme: OpenIdConnectDefaults.AuthenticationScheme
                    );

                    Log.Information("Azure Access Token acquired successfully.");
                    var email = User.Identity?.Name;
                    Log.Information($"User signed in: {email}");
                }

                return Redirect("/AssetCategories");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during Azure token acquisition.");
                throw;
            }
        }
    }
}
