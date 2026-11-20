using System.Text.Json;
using System.Text.Json.Serialization;
using AssetManagement.Application.Abstractions.Services;
using AssetManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web.UI;

namespace AssetManagement.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AssetManagement.Web.Options.MobileOptions>(configuration.GetSection(AssetManagement.Web.Options.MobileOptions.SectionName));
        services.AddOpenApiDocs();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddProblemDetails();

        services.AddRazorPages().AddMicrosoftIdentityUI();

        services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
            .AddMicrosoftIdentityUI()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

        services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

        return services;
    }
}
