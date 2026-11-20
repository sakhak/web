using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AssetManagement.Application.Abstractions.Services;
using AssetManagement.Application.Common.Models;
using AssetManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace AssetManagement.Web.Api;

/// <summary>
/// Makes a write safe to retry. The client sends a unique <c>Idempotency-Key</c> header per logical operation;
/// a repeat of the same request returns the stored successful response (header <c>Idempotent-Replayed: true</c>)
/// instead of applying the change again. Failed requests are not stored, so they can be retried with the same key.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotentAttribute : Attribute, IFilterFactory
{
    public const string HeaderName = "Idempotency-Key";
    public const string ReplayHeaderName = "Idempotent-Replayed";

    /// <summary>When true a request without the header is rejected with 400.</summary>
    public bool Required { get; set; }

    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<IdempotencyFilter>(serviceProvider, Required);
}

public sealed partial class IdempotencyFilter(
    IIdempotencyStore store,
    IOptions<JsonOptions> jsonOptions,
    ILogger<IdempotencyFilter> logger,
    bool required) : IAsyncResourceFilter
{
    private static readonly string[] SuccessContentTypes = ["application/json; charset=utf-8"];

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var http = context.HttpContext;
        var key = http.Request.Headers[IdempotentAttribute.HeaderName].ToString();

        if (string.IsNullOrEmpty(key))
        {
            if (!required)
            {
                await next();
                return;
            }

            context.Result = Fail($"The {IdempotentAttribute.HeaderName} header is required for this request.", HttpStatus.BadRequest);
            return;
        }

        if (!ValidKey().IsMatch(key))
        {
            context.Result = Fail($"{IdempotentAttribute.HeaderName} must be 8 to 64 characters: letters, digits, '-' or '_' (a GUID works).", HttpStatus.BadRequest);
            return;
        }

        var actor = http.User.Identity?.Name ?? http.User.FindFirst("StaffId")?.Value;
        if (string.IsNullOrEmpty(actor))
        {
            await next();
            return;
        }

        var hash = await HashRequestAsync(http.Request);

        IdempotencyDecision decision;
        try
        {
            decision = await store.BeginAsync(key, actor, http.Request.Method, http.Request.Path.Value ?? string.Empty, hash, http.RequestAborted);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The store is unavailable (for example the table is not created yet): keep serving writes without retry protection.
            logger.LogError(ex, "Idempotency store unavailable; processing {Path} without retry protection", http.Request.Path);
            await next();
            return;
        }

        switch (decision.Outcome)
        {
            case IdempotencyOutcome.Replay:
                http.Response.Headers[IdempotentAttribute.ReplayHeaderName] = "true";
                context.Result = new ContentResult
                {
                    StatusCode = decision.Response!.Status,
                    ContentType = decision.Response.ContentType,
                    Content = decision.Response.Body
                };
                return;

            case IdempotencyOutcome.Mismatch:
                context.Result = Fail($"This {IdempotentAttribute.HeaderName} was already used for a different request.", HttpStatus.Conflict);
                return;

            case IdempotencyOutcome.InProgress:
                http.Response.Headers.RetryAfter = "2";
                context.Result = Fail("The first request with this key is still being processed. Retry shortly.", HttpStatus.Conflict);
                return;
        }

        var executed = await next();
        await RecordOutcomeAsync(executed, key, actor);
    }

    private async Task RecordOutcomeAsync(ResourceExecutedContext executed, string key, string actor)
    {
        try
        {
            if (executed.Exception is not null && !executed.ExceptionHandled || !TryCapture(executed.Result, out var stored))
            {
                await store.AbandonAsync(key, actor, CancellationToken.None);
                return;
            }

            await store.CompleteAsync(key, actor, stored!, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not record the idempotency outcome for key {Key}", key);
        }
    }

    private bool TryCapture(IActionResult? result, out StoredResponse? stored)
    {
        stored = null;

        switch (result)
        {
            case ObjectResult { Value: not null } objectResult:
            {
                var status = objectResult.StatusCode ?? StatusCodes.Status200OK;
                if (status is < 200 or >= 300) return false;
                if (objectResult.Value is Result { Succeeded: false }) return false;

                var body = JsonSerializer.Serialize(objectResult.Value, objectResult.Value.GetType(), jsonOptions.Value.JsonSerializerOptions);
                stored = new StoredResponse(status, SuccessContentTypes[0], body);
                return true;
            }
            case StatusCodeResult { StatusCode: >= 200 and < 300 } statusResult:
                stored = new StoredResponse(statusResult.StatusCode, null, null);
                return true;
            default:
                return false;
        }
    }

    private static ObjectResult Fail(string message, HttpStatus status) =>
        new(Result<string>.Fail(message, status)) { StatusCode = (int)status };

    private static async Task<string> HashRequestAsync(HttpRequest request)
    {
        using var sha = SHA256.Create();
        var head = Encoding.UTF8.GetBytes($"{request.Method} {request.Path}{request.QueryString}\n");
        sha.TransformBlock(head, 0, head.Length, null, 0);

        if (request.HasFormContentType)
        {
            // Multipart boundaries change on every attempt, so hash the content instead of the raw body.
            var form = await request.ReadFormAsync();
            foreach (var field in form.OrderBy(f => f.Key, StringComparer.Ordinal))
            {
                var line = Encoding.UTF8.GetBytes($"{field.Key}={field.Value}\n");
                sha.TransformBlock(line, 0, line.Length, null, 0);
            }

            foreach (var file in form.Files.OrderBy(f => f.Name, StringComparer.Ordinal))
            {
                await using var stream = file.OpenReadStream();
                var fileHash = await SHA256.HashDataAsync(stream);
                var line = Encoding.UTF8.GetBytes($"{file.Name}:{file.FileName}:{file.Length}:{Convert.ToHexString(fileHash)}\n");
                sha.TransformBlock(line, 0, line.Length, null, 0);
            }
        }
        else
        {
            request.EnableBuffering();
            using var buffer = new MemoryStream();
            await request.Body.CopyToAsync(buffer);
            request.Body.Position = 0;
            var bytes = buffer.ToArray();
            sha.TransformBlock(bytes, 0, bytes.Length, null, 0);
        }

        sha.TransformFinalBlock([], 0, 0);
        return Convert.ToHexString(sha.Hash!);
    }

    [GeneratedRegex("^[A-Za-z0-9_-]{8,64}$")]
    private static partial Regex ValidKey();
}
