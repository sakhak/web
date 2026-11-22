using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AssetManagement.Web.Extensions;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Persistence;

namespace AssetManagement.Web.Pages
{
    [AllowAnonymous]
    public class LoginModel(
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration,
        IWebHostEnvironment environment) : PageModel
    {
        [BindProperty]
        public LocalLoginInput Input { get; set; } = new();

        [BindProperty]
        public string? ReturnUrl { get; set; }

        public bool UseLocalLogin => AuthenticationExtensions.IsLocalLoginEnabled(configuration, environment);

        public IActionResult OnGet(string? returnUrl = null)
        {
            ReturnUrl = GetSafeReturnUrl(returnUrl);
            return Page();
        }

        public IActionResult OnGetSignIn(string returnUrl = "/AzureCallback")
        {
            if (UseLocalLogin)
                return RedirectToPage(new { returnUrl });

            if (User.Identity?.IsAuthenticated == true)
                return Redirect(GetSafeReturnUrl(returnUrl));

            return Challenge(new AuthenticationProperties { RedirectUri = GetSafeReturnUrl(returnUrl) },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> OnPostLocalSignInAsync(CancellationToken cancellationToken)
        {
            if (!UseLocalLogin)
                return NotFound();

            ReturnUrl = GetSafeReturnUrl(ReturnUrl);
            if (!ModelState.IsValid)
                return Page();

            var username = Input.Username.Trim();
            var normalizedUsername = username.ToUpperInvariant();
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(
                candidate => candidate.Username != null &&
                             candidate.Username.Trim().ToUpper() == normalizedUsername,
                cancellationToken);

            var passwordHash = user?.Password?.Trim();
            if (user is null ||
                string.IsNullOrEmpty(passwordHash) ||
                !string.Equals(user.Active?.Trim(), "Y", StringComparison.OrdinalIgnoreCase) ||
                passwordHasher.VerifyHashedPassword(user, passwordHash, Input.Password) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password, or the account is inactive.");
                return Page();
            }

            var name = string.IsNullOrWhiteSpace(user.Mail) ? username : user.Mail.Trim();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, name),
                new Claim("LocalLogin", "true"),
                new Claim("LocalUsername", user.Username!.Trim())
            };
            var identity = new ClaimsIdentity(claims, AuthenticationExtensions.LocalCookieScheme);
            await HttpContext.SignInAsync(
                AuthenticationExtensions.LocalCookieScheme,
                new ClaimsPrincipal(identity));

            return LocalRedirect(ReturnUrl!);
        }

        public async Task<IActionResult> OnPostSignOutAsync()
        {
            if (!UseLocalLogin)
                return NotFound();

            await HttpContext.SignOutAsync(AuthenticationExtensions.LocalCookieScheme);
            return RedirectToPage();
        }

        private string GetSafeReturnUrl(string? returnUrl) =>
            !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Page("/Index") ?? "/";

        public sealed class LocalLoginInput
        {
            [System.ComponentModel.DataAnnotations.Required]
            public string Username { get; set; } = string.Empty;

            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }
    }
}
