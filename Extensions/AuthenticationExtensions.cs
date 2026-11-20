using System.Text;
using System.Text.Json;
using AssetManagement.Application.Common.Models;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Identity.Web;

namespace AssetManagement.Web.Extensions;

public static class AuthenticationExtensions
{
    public const string JwtScheme = "JwtAuth";
    public const string LocalCookieScheme = "LocalCookie";
    private const string FailureMessageKey = "JwtFailureMessage";

    public static bool IsLocalLoginEnabled(IConfiguration configuration, IHostEnvironment environment) =>
        environment.IsDevelopment() && configuration.GetValue<bool>("Authentication:LocalLogin:Enabled");

    public static IServiceCollection AddAuthenticationAndAuthorization(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useLocalLogin)
    {
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        if (useLocalLogin)
        {
            services.AddAuthentication(LocalCookieScheme)
                .AddCookie(LocalCookieScheme, options =>
                {
                    options.Cookie.Name = ".AssetManagement.Local";
                    options.LoginPath = "/Login";
                });
        }
        else
        {
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(configuration.GetSection("AzureADB2C"))
                .EnableTokenAcquisitionToCallDownstreamApi(["user.read"])
                .AddMicrosoftGraph(configuration.GetSection("DownstreamApis"))
                .AddDistributedTokenCaches();
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        services.AddAuthentication().AddJwtBearer(JwtScheme, _ => { });

        services.AddOptions<JwtBearerOptions>(JwtScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.HttpContext.Items[FailureMessageKey] =
                            context.Exception is SecurityTokenExpiredException ? "Token has expired" : "Invalid token";
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        var message = context.HttpContext.Items[FailureMessageKey] as string ?? "Token is missing or expired";
                        return WriteResultAsync(context.Response, Result<string>.Fail(message, HttpStatus.Unauthorized), jsonOptions);
                    },
                    OnForbidden = context => WriteResultAsync(context.Response,
                        Result<string>.Fail("You do not have permission to access this resource", HttpStatus.Forbidden), jsonOptions)
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static Task WriteResultAsync(HttpResponse response, Result<string> result, JsonSerializerOptions options)
    {
        response.StatusCode = StatusCodes.Status200OK;
        response.ContentType = "application/json";
        return response.WriteAsync(JsonSerializer.Serialize(result, options));
    }
}
