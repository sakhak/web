using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Web.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted && ExpectsJson(context.Request))
        {
            await WriteProblemAsync(context, ex);
        }
    }

    private static bool ExpectsJson(HttpRequest request) =>
        request.Path.StartsWithSegments("/api") ||
        request.Headers.Accept.Any(a => a?.Contains("json", StringComparison.OrdinalIgnoreCase) == true) ||
        string.Equals(request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        ProblemDetails problem;

        switch (exception)
        {
            case ValidationException validation:
                problem = new ValidationProblemDetails(
                    validation.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred."
                };
                break;

            case UnauthorizedAccessException:
                problem = new ProblemDetails { Status = StatusCodes.Status401Unauthorized, Title = "Unauthorized" };
                break;

            case KeyNotFoundException:
                problem = new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Not Found" };
                break;

            case OperationCanceledException when context.RequestAborted.IsCancellationRequested:
                return;

            default:
                logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                    Detail = environment.IsDevelopment() ? exception.Message : null
                };
                break;
        }

        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, problem.GetType(), (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json");
    }
}
