using AssetManagement.Web.Api;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AssetManagement.Web.Extensions;

public static class OpenApiExtensions
{
    public const string DocumentName = "v1";

    public static IServiceCollection AddOpenApiDocs(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(DocumentName, new OpenApiInfo
            {
                Title = "Asset Management API",
                Version = DocumentName,
                Description = "JSON API used by the mobile app. Sign in with POST /api/Auth/login, then send the returned access token as a Bearer token. " +
                              "Responses use the envelope { status, message, succeeded, data }. A missing or expired token and a missing permission are returned as HTTP 200 " +
                              "with succeeded=false and status 401/403 in the body."
            });

            options.DocInclusionPredicate((_, api) => api.RelativePath?.StartsWith("api/", StringComparison.OrdinalIgnoreCase) == true);
            options.CustomSchemaIds(type => type.FullName?.Replace("AssetManagement.", string.Empty).Replace('+', '.'));

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Access token from POST /api/Auth/login."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = []
            });

            options.OperationFilter<IdempotencyKeyHeaderFilter>();

            var xml = Path.Combine(AppContext.BaseDirectory, $"{typeof(OpenApiExtensions).Assembly.GetName().Name}.xml");
            if (File.Exists(xml)) options.IncludeXmlComments(xml);
        });

        return services;
    }

    public static bool OpenApiEnabled(this WebApplication app) =>
        app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Configuration.GetValue<bool>("OpenApi:Enabled");

    public static WebApplication UseOpenApiDocs(this WebApplication app)
    {
        if (!app.OpenApiEnabled()) return app;

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{DocumentName}/swagger.json", "Asset Management API v1");
            options.RoutePrefix = "swagger";
            options.EnablePersistAuthorization();
        });

        return app;
    }
}

/// <summary>Documents the Idempotency-Key header on actions marked [Idempotent].</summary>
public sealed class IdempotencyKeyHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var attribute = context.MethodInfo.GetCustomAttributes(typeof(IdempotentAttribute), inherit: true)
            .OfType<IdempotentAttribute>().FirstOrDefault();
        if (attribute is null) return;

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = IdempotentAttribute.HeaderName,
            In = ParameterLocation.Header,
            Required = attribute.Required,
            Description = "Unique id (a GUID works) for this logical operation. Resend the same value when retrying: " +
                          "a request that already succeeded returns the stored response with header Idempotent-Replayed: true. " +
                          "Reusing a key with a different request returns 409.",
            Schema = new OpenApiSchema { Type = "string", MinLength = 8, MaxLength = 64 }
        });
    }
}
