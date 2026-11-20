using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AssetManagement.Application.Abstractions.Services;
using AssetManagement.Application.Common.Constants;
using AssetManagement.Domain.Entities;
using Microsoft.AspNetCore.WebUtilities;

namespace AssetManagement.Web.Middleware;

public sealed class AuditLoggingMiddleware(
    RequestDelegate next,
    IServiceScopeFactory scopeFactory,
    ILogger<AuditLoggingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (!AuditRules.ShouldLog(httpContext.Request.Path, httpContext.Request.Method))
        {
            await next(httpContext);
            return;
        }

        var queryParams = httpContext.Request.QueryString.HasValue ? httpContext.Request.QueryString.Value : null;
        var requestBody = await ReadRequestBodyAsync(httpContext.Request);

        var sw = Stopwatch.StartNew();
        Exception? caught = null;

        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            caught = ex;
            throw;
        }
        finally
        {
            sw.Stop();
            await WriteLogAsync(httpContext, queryParams, requestBody, caught, sw.ElapsedMilliseconds);
        }
    }

    private async Task WriteLogAsync(HttpContext httpContext, string? queryParams, string? requestBody, Exception? caught, long elapsedMs)
    {
        try
        {
            var statusCode = caught is not null ? 500 : httpContext.Response.StatusCode;
            var description = caught is not null
                ? $"{ApplicationConstants.MessageResponse.Failed} — {caught.Message}"
                : httpContext.Items[ApplicationConstants.LogKey.Description] as string
                  ?? (statusCode >= 400 ? $"Failed ({statusCode})" : null);

            using var scope = scopeFactory.CreateScope();
            var writer = scope.ServiceProvider.GetRequiredService<ISystemLogWriter>();

            await writer.LogAsync(new SystemLog
            {
                Module = AuditRules.ModuleFor(httpContext.Request.Path),
                Action = AuditRules.ActionFor(httpContext.Request.Path),
                Url = httpContext.Request.Path.Value,
                Method = httpContext.Request.Method,
                QueryParams = queryParams,
                RequestBody = requestBody,
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "-",
                UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
                ResponseStatus = statusCode,
                ResponseMessage = ReasonPhrases.GetReasonPhrase(statusCode),
                Duration = elapsedMs,
                Description = description,
                CreatedBy = httpContext.User?.Identity?.Name ?? "Anonymous",
                CreatedOn = DateTime.Now
            });
        }
        catch (Exception logEx)
        {
            logger.LogError(logEx, "Failed to record system log");
        }
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!HttpMethods.IsPost(request.Method) && !HttpMethods.IsPut(request.Method)) return null;

        if (request.HasFormContentType)
        {
            var form = await request.ReadFormAsync();
            var dict = form
                .Where(f => !AuditRules.IsSensitiveKey(f.Key))
                .ToDictionary(f => f.Key, f => f.Value.ToString());

            return Truncate(JsonSerializer.Serialize(dict, JsonOptions));
        }

        request.EnableBuffering();
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var raw = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return Truncate(AuditRules.RedactJson(raw));
    }

    private static string Truncate(string value) => value.Length > 4000 ? value[..4000] + "\n  ..." : value;
}
