using System.Text.RegularExpressions;

namespace AssetManagement.Web.Middleware;

public static partial class AuditRules
{
    private static readonly string[] SkippedPrefixes =
        ["/health", "/lib", "/css", "/js", "/img", "/logs", "/favicon", "/MicrosoftIdentity", "/signin-oidc", "/signout-callback-oidc", "/api/Auth", "/Login", "/AzureCallback", "/Error"];

    private static readonly string[] ReadLoggedPrefixes = ["/Assets", "/Reports"];

    private static readonly Dictionary<string, string> Modules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Assets"] = "Asset",
        ["Asset"] = "Asset",
        ["AssetMovements"] = "Movement",
        ["AssetMovement"] = "Movement",
        ["AssetCategories"] = "Category",
        ["Onboarding"] = "Onboarding",
        ["PurchaseOrder"] = "Onboarding",
        ["GoodsReceipt"] = "Onboarding",
        ["DepreciationProfile"] = "Depreciation",
        ["Supplier"] = "Supplier",
        ["Reports"] = "Report",
        ["Report"] = "Report",
        ["Admin"] = "Admin",
        ["MasterData"] = "MasterData"
    };

    public static bool ShouldLog(PathString path, string method)
    {
        var value = path.Value ?? string.Empty;
        if (SkippedPrefixes.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase))) return false;
        if (value.Length > 1 && Path.HasExtension(value)) return false;

        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method))
            return ReadLoggedPrefixes.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));

        return !HttpMethods.IsOptions(method);
    }

    public static string ModuleFor(PathString path)
    {
        var parts = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (parts.Length == 0) return "Home";

        var key = parts[0].Equals("api", StringComparison.OrdinalIgnoreCase) && parts.Length > 1 ? parts[1] : parts[0];
        return Modules.GetValueOrDefault(key, key);
    }

    public static string ActionFor(PathString path)
    {
        var parts = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (parts.Length > 0 && parts[0].Equals("api", StringComparison.OrdinalIgnoreCase)) parts = parts[1..];
        return parts.Length > 1 ? string.Join('/', parts[1..]) : "Index";
    }

    public static bool IsSensitiveKey(string key) =>
        key.Contains("password", StringComparison.OrdinalIgnoreCase)
        || key.Contains("token", StringComparison.OrdinalIgnoreCase)
        || key.Contains("secret", StringComparison.OrdinalIgnoreCase)
        || key.Contains("authorization", StringComparison.OrdinalIgnoreCase);

    public static string RedactJson(string raw) => SensitiveJsonValue().Replace(raw, "$1\"***\"");

    [GeneratedRegex(@"(""[^""]*(?:password|token|secret|authorization)[^""]*""\s*:\s*)""[^""]*""", RegexOptions.IgnoreCase)]
    private static partial Regex SensitiveJsonValue();
}
